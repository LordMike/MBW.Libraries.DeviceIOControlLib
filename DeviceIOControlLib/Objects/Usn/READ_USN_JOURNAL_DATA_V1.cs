using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Usn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct READ_USN_JOURNAL_DATA_V1
    {
        public USN StartUsn;
        public UsnJournalReasonMask ReasonMask;
        public int ReturnOnlyOnClose;
        public long Timeout;
        public long BytesToWaitFor;
        public long UsnJournalId;
        public short MinMajorVersion;
        public short MaxMajorVersion;
    }
}