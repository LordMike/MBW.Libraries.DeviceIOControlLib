using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.FileSystem;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public static class DeviceIoControlHelper
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode IoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [In] object InBuffer,
            uint nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            [In] IntPtr Overlapped
            );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode ioControlCode,
            byte[] inBuffer,
            uint nInBufferSize,
            byte[] outBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr overlapped
            );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode IoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [In] object InBuffer,
            uint nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            ref NativeOverlapped Overlapped
            );

        public static Task<byte[]> InvokeIoControlAsync<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode)
        {
            return InvokeIoControlAsync(handle, controlCode, outputLength, input, out errorCode, CancellationToken.None);
        }

        public static Task<byte[]> InvokeIoControlAsync<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode, CancellationToken cancellationToken)
        {
            uint returnedBytes = 0;
            uint inputSize = (uint)Marshal.SizeOf(input);

            ManualResetEvent stateChangeEvent = new ManualResetEvent(false);
            NativeOverlapped nativeOverlapped = new NativeOverlapped();
            nativeOverlapped.EventHandle = stateChangeEvent.SafeWaitHandle.DangerousGetHandle();

            errorCode = 0;
            byte[] output = new byte[outputLength];

            Stopwatch swData = Stopwatch.StartNew();
            Stopwatch swBegin = Stopwatch.StartNew();

            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, ref nativeOverlapped);

            if (!success)
                Debugger.Break();

            swBegin.Stop();

            Task tsk = stateChangeEvent.AsTask(cancellationToken);

            Task<byte[]> secTsk = tsk.ContinueWith(_ =>
            {
                swData.Stop();

                Console.WriteLine(swData.Elapsed);
                Console.WriteLine(swBegin.Elapsed);

                return output;
            });

            return secTsk;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input or output.
        /// </summary>
        /// <returns>Success</returns>
        public static bool InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode)
        {
            uint returnedBytes = 0;

            return DeviceIoControl(handle, controlCode, null, 0, null, 0, ref returnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of a byte array.
        /// </summary>
        public static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength)
        {
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of a byte array. Lets the caller handle the errorcode (if any).
        /// </summary>
        public static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, out int errorCode)
        {
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

            errorCode = 0;

            if (!success)
                errorCode = Marshal.GetLastWin32Error();

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of an object of type T.
        /// </summary>
        public static T InvokeIoControl<T>(SafeFileHandle handle, IOControlCode controlCode)
        {
            uint returnedBytes = 0;

            object output = default(T);
            uint outputSize = (uint)Marshal.SizeOf(output);
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return (T)output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieve the output in the form of an object of type T.
        /// </summary>
        public static T InvokeIoControl<T, V>(SafeFileHandle handle, IOControlCode controlCode, V input)
        {
            uint returnedBytes = 0;

            object output = default(T);
            uint outputSize = (uint)Marshal.SizeOf(output);

            uint inputSize = (uint)Marshal.SizeOf(input);
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return (T)output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieves no output.
        /// </summary>
        public static void InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, V input)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)Marshal.SizeOf(input);
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, null, 0, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }
        }

        /// <summary>
        /// Calls InvokeIoControl with the specified input, returning a byte array. It allows the caller to handle errors.
        /// </summary>
        public static byte[] InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode)
        {
            uint returnedBytes = 0;
            uint inputSize = (uint)Marshal.SizeOf(input);

            errorCode = 0;
            byte[] output = new byte[outputLength];

            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                errorCode = Marshal.GetLastWin32Error();
            }

            return output;
        }

        /// <summary>
        /// Repeatedly invokes InvokeIoControl, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        public static byte[] InvokeIoControlUnknownSize(SafeFileHandle handle, IOControlCode controlCode, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint outputLength = increment;

            do
            {
                byte[] output = new byte[outputLength];
                bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == 234)
                    {
                        // More data
                        outputLength += increment;
                        continue;
                    }

                    throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                if (output.Length == returnedBytes)
                    return output;

                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, returnedBytes);

                return res;
            } while (true);
        }

        /// <summary>
        /// Repeatedly invokes InvokeIoControl with the specified input, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        public static byte[] InvokeIoControlUnknownSize<V>(SafeFileHandle handle, IOControlCode controlCode, V input, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)Marshal.SizeOf(input);
            uint outputLength = increment;

            do
            {
                byte[] output = new byte[outputLength];
                bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == 234)
                    {
                        // More data
                        outputLength += increment;
                        continue;
                    }

                    throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                if (output.Length == returnedBytes)
                    return output;

                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, returnedBytes);

                return res;
            } while (true);
        }

    }
}