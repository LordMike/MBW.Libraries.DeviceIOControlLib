using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DRIVE_LAYOUT_INFORMATION_EX_INTERNAL
    {
        [MarshalAs(UnmanagedType.U4)]
        public PartitionStyle PartitionStyle;
        public int PartitionCount;
        public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformaiton;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 128)]
        public PARTITION_INFORMATION_EX[] PartitionEntry;
    }
}