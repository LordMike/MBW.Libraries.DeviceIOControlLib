using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FILE_ALLOCATED_RANGE_BUFFER
    {
        public long FileOffset;
        public long Length;
    }
}