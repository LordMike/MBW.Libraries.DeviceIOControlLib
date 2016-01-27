using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Disk
{
    public struct DRIVE_LAYOUT_INFORMATION_EX
    {
        public PartitionStyle PartitionStyle;
        public int PartitionCount;
        public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformaiton;
        public PARTITION_INFORMATION_EX[] PartitionEntry;
    }
}