using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_GEOMETRY_EX
    {
        public DISK_GEOMETRY Geometry;
        public long DiskSize;
        public DISK_PARTITION_INFO PartitionInformation;
        public DISK_EX_INT13_INFO DiskInt13Info;
    }
}