using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_CACHE_INFORMATION
    {
        public bool ParametersSavable;
        public bool ReadCacheEnabled;
        public bool WriteCacheEnabled;
        public DISK_CACHE_RETENTION_PRIORITY ReadRetentionPriority;
        public DISK_CACHE_RETENTION_PRIORITY WriteRetentionPriority;
        public short DisablePrefetchTransferLength;
        public bool PrefetchScalar;

        public DISK_CACHE_INFORMATION_UNION DiskCacheInformationUnion { get; set; }
    }
}