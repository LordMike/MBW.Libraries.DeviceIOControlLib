using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_STATISTICS_Allocate
    {
        public uint Calls;
        public uint Clusters;
        public uint Hints;
        public uint RunsReturned;
        public uint HintsHonored;
        public uint HintsClusters;
        public uint Cache;
        public uint CacheClusters;
        public uint CacheMiss;
        public uint CacheMissClusters;
    }
}