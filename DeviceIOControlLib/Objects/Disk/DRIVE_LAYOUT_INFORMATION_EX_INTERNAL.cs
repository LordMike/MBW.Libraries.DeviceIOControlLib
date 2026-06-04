using System.Runtime.CompilerServices;
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
    }
}