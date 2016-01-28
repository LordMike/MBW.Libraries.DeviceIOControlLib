using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Usn
{
    public interface IUSN_RECORD
    {
        uint RecordLength { get; set; }
        ushort MajorVersion { get; set; }
        ushort MinorVersion { get; set; }
        USN Usn { get; set; }
        ulong TimeStamp { get; set; }
        UsnJournalReasonMask Reason { get; set; }
        USN_SOURCE_INFO SourceInfo { get; set; }
        uint SecurityId { get; set; }
        FileAttributes FileAttributes { get; set; }
        ushort FileNameLength { get; set; }
        ushort FileNameOffset { get; set; }
        string FileName { get; set; }
    }
}