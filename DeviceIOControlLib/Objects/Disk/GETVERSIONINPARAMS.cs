using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GETVERSIONINPARAMS
    {
        public byte bVersion;
        public byte bRevision;
        public byte bReserved;
        public byte bIDEDeviceMap;
        public ulong fCapabilities;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ulong[] dwReserved;
    }
}