using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DRIVE_LAYOUT_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_MBR Mbr;
    }
}