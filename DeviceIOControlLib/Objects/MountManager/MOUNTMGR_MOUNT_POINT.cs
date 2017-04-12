using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.MountManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUNTMGR_MOUNT_POINT
    {
        public uint SymbolicLinkNameOffset;
        public ushort SymbolicLinkNameLength;
        public uint UniqueIdOffset;
        public ushort UniqueIdLength;
        public uint DeviceNameOffset;
        public ushort DeviceNameLength;
    }
}