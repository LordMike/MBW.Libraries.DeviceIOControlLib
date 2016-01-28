using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Usn
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct USN_RECORD_V2 : IUSN_RECORD
    {
        private uint _recordLength;
        private ushort _majorVersion;
        private ushort _minorVersion;
        private ulong _fileReferenceNumber;
        private ulong _parentFileReferenceNumber;
        private long _usn;
        private ulong _timeStamp;
        private UsnJournalReasonMask _reason;
        private USN_SOURCE_INFO _sourceInfo;
        private uint _securityId;
        private FileAttributes _fileAttributes;
        private ushort _fileNameLength;
        private ushort _fileNameOffset;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
        private string _fileName;

        public uint RecordLength
        {
            get { return _recordLength; }
            set { _recordLength = value; }
        }
        public ushort MajorVersion
        {
            get { return _majorVersion; }
            set { _majorVersion = value; }
        }
        public ushort MinorVersion
        {
            get { return _minorVersion; }
            set { _minorVersion = value; }
        }
        public ulong FileReferenceNumber
        {
            get { return _fileReferenceNumber; }
            set { _fileReferenceNumber = value; }
        }
        public ulong ParentFileReferenceNumber
        {
            get { return _parentFileReferenceNumber; }
            set { _parentFileReferenceNumber = value; }
        }
        public USN Usn
        {
            get { return _usn; }
            set { _usn = value; }
        }
        public ulong TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        public UsnJournalReasonMask Reason
        {
            get { return _reason; }
            set { _reason = value; }
        }
        public USN_SOURCE_INFO SourceInfo
        {
            get { return _sourceInfo; }
            set { _sourceInfo = value; }
        }
        public uint SecurityId
        {
            get { return _securityId; }
            set { _securityId = value; }
        }
        public FileAttributes FileAttributes
        {
            get { return _fileAttributes; }
            set { _fileAttributes = value; }
        }
        public ushort FileNameLength
        {
            get { return _fileNameLength; }
            set { _fileNameLength = value; }
        }
        public ushort FileNameOffset
        {
            get { return _fileNameOffset; }
            set { _fileNameOffset = value; }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
    }
}