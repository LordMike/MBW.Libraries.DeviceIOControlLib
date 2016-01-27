using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Usn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USN_JOURNAL_DATA_V1
    {
        public long UsnJournalID;
        public USN FirstUsn;
        public USN NextUsn;
        public USN LowestValidUsn;
        public USN MaxUsn;
        public long MaximumSize;
        public long AllocationDelta;
        public short MinSupportedMajorVersion;
        public short MaxSupportedMajorVersion;
    }
}