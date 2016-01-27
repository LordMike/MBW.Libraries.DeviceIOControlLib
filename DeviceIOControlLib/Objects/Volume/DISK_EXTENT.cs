using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Volume
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_EXTENT
    {
        public uint DiskNumber;
        public long StartingOffset;
        public long ExtentLength;
    }
}