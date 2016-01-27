using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USN_JOURNAL_DATA_V0
    {
        public long UsnJournalID;
        public USN FirstUsn;
        public USN NextUsn;
        public USN LowestValidUsn;
        public USN MaxUsn;
        public long MaximumSize;
        public long AllocationDelta;
    }
}