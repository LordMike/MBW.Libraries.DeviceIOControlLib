using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOVE_FILE_DATA
    {
        public IntPtr FileHandle;
        public ulong StartingVcn;
        public ulong StartingLcn;
        public uint ClusterCount;
    }
}