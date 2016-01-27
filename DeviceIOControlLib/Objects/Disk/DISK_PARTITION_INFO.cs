using System;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DISK_PARTITION_INFO
    {
        [FieldOffset(0)]
        public int SizeOfPartitionInfo;
        [FieldOffset(4)]
        public PartitionStyle PartitionStyle;
        [FieldOffset(8)]
        public uint MbrSignature;
        [FieldOffset(8)]
        public Guid GptGuidId;
    }
}