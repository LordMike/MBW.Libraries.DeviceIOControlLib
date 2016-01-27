using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DRIVE_LAYOUT_INFORMATION_GPT
    {
        public Guid DiskId;
        public long StartingUsableOffset;
        public long UsableLength;
        public ulong MaxPartitionCount;
    }
}