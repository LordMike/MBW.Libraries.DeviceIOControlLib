using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION
    {
        public long StartingOffset;
        public long PartitionLength;
        public int HiddenSectors;
        public int PartitionNumber;
        public byte PartitionType;
        [MarshalAs(UnmanagedType.I1)]
        public bool BootIndicator;
        [MarshalAs(UnmanagedType.I1)]
        public bool RecognizedPartition;
        [MarshalAs(UnmanagedType.I1)]
        public bool RewritePartition;
    }
}