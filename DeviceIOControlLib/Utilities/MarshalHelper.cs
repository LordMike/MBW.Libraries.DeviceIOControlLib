using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Utilities
{
    internal static class MarshalHelper
    {
        public static T ToStructure<T>(this IntPtr ptr)
        {
#if NETCORE
            return Marshal.PtrToStructure<T>(ptr);
#else
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
#endif
        }

        public static uint SizeOf<T>()
        {
#if NETCORE
            return (uint)Marshal.SizeOf<T>();
#else
            return (uint)Marshal.SizeOf(typeof(T));
#endif
        }
    }
}