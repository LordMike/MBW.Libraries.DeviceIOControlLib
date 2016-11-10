using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceIOControlLib.Utilities
{
    internal static class Utils
    {
        public static string GetWin32ErrorMessage(int errorCode)
        {
            return new Win32Exception(errorCode).Message;
        }

        public static T ByteArrayToStruct<T>(byte[] data, int index) where T : struct
        {
            using (UnmanagedMemory mem = new UnmanagedMemory(data.Length - index))
            {
                Marshal.Copy(data, index, mem.Handle, data.Length - index);
                return mem.Handle.ToStructure<T>();
            }
        }

        public static string ReadNullTerminatedString(byte[] br, int index, Encoding encoding, int maxSize = 255)
        {
            if (maxSize <= 0)
                return string.Empty;

            byte[] nameBytes = br;
            for (int i = index; i < nameBytes.Length; i++)
            {
                if (nameBytes[i] == 0) // \0
                {
                    return encoding.GetString(nameBytes, index, i - index);
                }
            }

            return string.Empty;
        }
    }
}
