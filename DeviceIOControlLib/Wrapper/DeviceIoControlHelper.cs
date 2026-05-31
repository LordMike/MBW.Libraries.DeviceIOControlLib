using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public static partial class DeviceIoControlHelper
    {
        [LibraryImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode ioControlCode,
            IntPtr inBuffer,
            uint nInBufferSize,
            IntPtr outBuffer,
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

            return DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, IntPtr.Zero, 0, ref returnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of a byte array.
        /// </summary>
        public unsafe static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength)
        {
            bool success;
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            fixed (byte* outputPtr = output)
            {
                success = DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
            }

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
        public unsafe static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, out int errorCode)
        {
            bool success;
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            fixed (byte* outputPtr = output)
            {
                success = DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
            }

            errorCode = 0;

            if (!success)
                errorCode = Marshal.GetLastWin32Error();

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of an object of type T.
        /// </summary>
        public unsafe static T InvokeIoControl<T>(SafeFileHandle handle, IOControlCode controlCode) 
            where T : unmanaged
        {
            uint returnedBytes = 0;

            T output = default;
            uint outputSize = MarshalHelper.SizeOf<T>();
            bool success = DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, new IntPtr(&output), outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieve the output in the form of an object of type T.
        /// </summary>
        public unsafe static T InvokeIoControl<T, V>(SafeFileHandle handle, IOControlCode controlCode, V input) 
            where T : unmanaged 
            where V : unmanaged
        {
            uint returnedBytes = 0;

            T output = default;
            uint outputSize = MarshalHelper.SizeOf<T>();

            uint inputSize = MarshalHelper.SizeOf<V>();
            bool success = DeviceIoControl(handle, controlCode, new IntPtr(&input), inputSize, new IntPtr(&output), outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieves no output.
        /// </summary>
        public unsafe static void InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, V input)
            where V : unmanaged
        {
            uint returnedBytes = 0;

            uint inputSize = MarshalHelper.SizeOf<V>();
            bool success = DeviceIoControl(handle, controlCode, new IntPtr(&input), inputSize, IntPtr.Zero, 0, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }
        }

        /// <summary>
        /// Calls InvokeIoControl with the specified input, returning a byte array. It allows the caller to handle errors.
        /// </summary>
        public unsafe static byte[] InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode)
            where V : unmanaged
        {
            bool success;
            uint returnedBytes = 0;
            uint inputSize = MarshalHelper.SizeOf<V>();

            errorCode = 0;

            byte[] output = new byte[outputLength];
            fixed (byte* outputPtr = output)
            {
                success = DeviceIoControl(handle, controlCode, new IntPtr(&input), inputSize, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
            }

            if (!success)
            {
                errorCode = Marshal.GetLastWin32Error();
            }

            return output;
        }

        /// <summary>
        /// Repeatedly invokes InvokeIoControl, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        public unsafe static byte[] InvokeIoControlUnknownSize(SafeFileHandle handle, IOControlCode controlCode, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint outputLength = increment;

            byte[] output = new byte[outputLength];

            do
            {
                bool success;
                fixed(byte* outputPtr = output)
                {
                    success = DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
                }

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
        public unsafe static byte[] InvokeIoControlUnknownSize<V>(SafeFileHandle handle, IOControlCode controlCode, V input, uint increment = 128, uint inputSizeOverride = 0)
            where V : unmanaged
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

            byte[] output = new byte[outputLength];

            do
            {
                bool success;
                fixed(byte* outputPtr = output)
                {
                    success = DeviceIoControl(handle, controlCode, new IntPtr(&input), inputSize, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
                }

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
        public unsafe static byte[] InvokeIoControlUnknownSize(SafeFileHandle handle, IOControlCode controlCode, byte[] input, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)input.Length;
            uint outputLength = increment;

            byte[] output = new byte[outputLength];

            do
            {
                bool success;
                fixed (byte* inputPtr = input)
                fixed (byte* outputPtr = output)
                {
                    success = DeviceIoControl(handle, controlCode, new IntPtr(inputPtr), inputSize, new IntPtr(outputPtr), outputLength, ref returnedBytes, IntPtr.Zero);
                }

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