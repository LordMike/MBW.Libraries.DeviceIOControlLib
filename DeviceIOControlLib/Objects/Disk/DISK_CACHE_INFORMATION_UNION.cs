using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DISK_CACHE_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public SCALAR_PREFETCH ScalarPrefetch;
        [FieldOffset(0)]
        public BLOCK_PREFETCH BlockPrefetch;
    }
}