using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION_MBR
    {
        public byte PartitionType;
        [MarshalAs(UnmanagedType.U1)]
        public bool BootIndicator;
        [MarshalAs(UnmanagedType.U1)]
        public bool RecognizedPartition;
        public uint HiddenSectors;
    }
}