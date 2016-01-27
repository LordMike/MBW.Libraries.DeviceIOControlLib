using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PARTITION_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public PARTITION_INFORMATION_GPT Gpt;
        [FieldOffset(0)]
        public PARTITION_INFORMATION_MBR Mbr;
    }
}