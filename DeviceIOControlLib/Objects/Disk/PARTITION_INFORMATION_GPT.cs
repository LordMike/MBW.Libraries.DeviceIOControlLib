using System;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct PARTITION_INFORMATION_GPT
    {
        [FieldOffset(0)]
        public Guid PartitionType;
        [FieldOffset(16)]
        public Guid PartitionId;
        [FieldOffset(32)]
        [MarshalAs(UnmanagedType.U8)]
        public EFIPartitionAttributes Attributes;
        [FieldOffset(40)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Name;
    }
}