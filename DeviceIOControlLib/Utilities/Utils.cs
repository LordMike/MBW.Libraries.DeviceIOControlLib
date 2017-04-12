using System.Collections.Generic;
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

        public static string ReadNullTerminatedAsciiString(byte[] br, int index)
        {
            byte[] nameBytes = br;
            for (int i = index; i < nameBytes.Length; i++)
            {
                if (nameBytes[i] == 0) // \0
                {
                    return Encoding.ASCII.GetString(nameBytes, index, i - index);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads strings terminated by null, until it hits a double-null (empty string)
        /// </summary>
        public static string ReadNullTerminatedUnicodeString(byte[] br, int index)
        {
            int idx = index;

            // Locate next Unicode null
            for (int i = index; i < br.Length; i += 2)
            {
                if (br[i] == 0 && br[i + 1] == 0) // UTF-16 null
                {
                    return Encoding.Unicode.GetString(br, index, i - index);
                }
            }

            return null;
        }

        /// <summary>
        /// Reads strings terminated by null, until it hits an empty string
        /// </summary>
        public static IEnumerable<string> ReadUnicodeStringArray(byte[] br, int index)
        {
            int idx = index;

            while (true)
            {
                string str = ReadNullTerminatedUnicodeString(br, idx);

                if (string.IsNullOrEmpty(str))
                    yield break;

                idx += Encoding.Unicode.GetByteCount(str) + 2;

                yield return str;
            }
        }
    }
}
