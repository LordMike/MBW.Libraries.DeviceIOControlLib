using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Utilities
{
    internal static class MarshalHelper
    {
        public static unsafe T ToStructure<T>(this IntPtr ptr) where T : unmanaged
        {
            return *(T*)ptr.ToPointer();
        }

        public static unsafe uint SizeOf<T>() where T : unmanaged
        {
            return (uint)sizeof(T);
        }
    }
}