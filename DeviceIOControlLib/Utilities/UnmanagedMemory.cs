using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Utilities
{
    public class UnmanagedMemory : IDisposable
    {
        public IntPtr Handle { get; }

        public UnmanagedMemory(int size)
        {
            Handle = Marshal.AllocHGlobal(size);
        }

        public UnmanagedMemory(byte[] data)
        {
            Handle = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Handle, data.Length);
        }

        public static implicit operator IntPtr(UnmanagedMemory mem)
        {
            return mem.Handle;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(this);
        }
    }
}