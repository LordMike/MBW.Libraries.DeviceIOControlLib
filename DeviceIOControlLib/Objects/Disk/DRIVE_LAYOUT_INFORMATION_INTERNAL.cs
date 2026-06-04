using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DRIVE_LAYOUT_INFORMATION_INTERNAL
    {
        public int PartitionCount;
        public uint Signature;
    }
}