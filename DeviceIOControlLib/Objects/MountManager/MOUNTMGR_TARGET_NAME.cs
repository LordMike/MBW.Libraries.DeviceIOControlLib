using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.MountManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUNTMGR_TARGET_NAME
    {
        public ushort DeviceNameLength;

        [MarshalAs(UnmanagedType.ByValArray)]
        public byte[] DeviceName;
    }
}