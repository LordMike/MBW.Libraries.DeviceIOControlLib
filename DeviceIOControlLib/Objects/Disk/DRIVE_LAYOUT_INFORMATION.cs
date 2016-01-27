namespace DeviceIOControlLib.Objects.Disk
{
    public struct DRIVE_LAYOUT_INFORMATION
    {
        public int PartitionCount;
        public uint Signature;
        public PARTITION_INFORMATION[] PartitionEntry;
    }
}