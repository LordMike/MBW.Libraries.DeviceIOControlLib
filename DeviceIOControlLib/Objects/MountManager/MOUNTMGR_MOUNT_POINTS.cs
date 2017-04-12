using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.MountManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUNTMGR_MOUNT_POINTS
    {
        public uint Size;
        public uint NumberOfMountPoints;
        public MOUNTMGR_MOUNT_POINT[] MountPoints;
    }
}