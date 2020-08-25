using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public static class DeviceIoControlHelper
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
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

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
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
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
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
            uint outputSize = MarshalHelper.SizeOf<T>();
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
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
            uint outputSize = MarshalHelper.SizeOf<T>();

            uint inputSize = MarshalHelper.SizeOf<V>();
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return (T)output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieves no output.
        /// </summary>
        public static void InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, V input)
        {
            uint returnedBytes = 0;

            uint inputSize = MarshalHelper.SizeOf<V>();
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, null, 0, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }
        }

        /// <summary>
        /// Calls InvokeIoControl with the specified input, returning a byte array. It allows the caller to handle errors.
        /// </summary>
        public static byte[] InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode)
        {
            uint returnedBytes = 0;
            uint inputSize = MarshalHelper.SizeOf<V>();

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

                    throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                if (output.Length == returnedBytes)
                    return output;

                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, (int)returnedBytes);

                return res;
            } while (true);
        }

        /// <summary>
        /// Repeatedly invokes InvokeIoControl with the specified input, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        public static byte[] InvokeIoControlUnknownSize<V>(SafeFileHandle handle, IOControlCode controlCode, V input, uint increment = 128, uint inputSizeOverride = 0)
        {
            uint returnedBytes = 0;

            uint inputSize;
            uint outputLength = increment;

            if (inputSizeOverride > 0)
            {
                inputSize = inputSizeOverride;
            }
            else
            {
                inputSize = MarshalHelper.SizeOf<V>();
            }

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

                    throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                if (output.Length == returnedBytes)
                    return output;

                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, (int)returnedBytes);

                return res;
            } while (true);
        }

    }
}