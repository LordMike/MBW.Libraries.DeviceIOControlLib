using System;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;

namespace DeviceIOControlLib.Objects.Usn
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct USN_RECORD_V3 : IUSN_RECORD
    {
        internal uint _recordLength;
        internal ushort _majorVersion;
        internal ushort _minorVersion;
        public fixed byte _fileReferenceNumber[16];
        public fixed byte _parentFileReferenceNumber[16];
        public long _usn;
        public ulong _timeStamp;
        public UsnJournalReasonMask _reason;
        public USN_SOURCE_INFO _sourceInfo;
        public uint _securityId;
        public FileAttributes _fileAttributes;
        public ushort _fileNameLength;
        public ushort _fileNameOffset;
        public fixed char _fileName[1];

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
        public byte[] FileReferenceNumber
        {
            get
            {
                byte[] fileReferenceNumber = new byte[16];
                fixed (byte* fileReferenceNumberDst = fileReferenceNumber)
                fixed (byte* fileReferenceNumberSrc = _fileReferenceNumber)
                {
                    Buffer.MemoryCopy(fileReferenceNumberSrc, fileReferenceNumberDst, 16, 16);
                }

                return fileReferenceNumber;
            }
            set 
            {
                fixed (byte* fileReferenceNumberDst = _fileReferenceNumber)
                fixed (byte* fileReferenceNumberSrc = value)
                {
                    Buffer.MemoryCopy(fileReferenceNumberSrc, fileReferenceNumberDst, 16, 16);
                }
            }
        }

        public byte[] ParentFileReferenceNumber
        {
            get
            {
                byte[] parentFileReferenceNumber = new byte[16];
                fixed (byte* parentFileReferenceNumberDst = parentFileReferenceNumber)
                fixed (byte* parentFileReferenceNumberSrc = _parentFileReferenceNumber)
                {
                    Buffer.MemoryCopy(parentFileReferenceNumberSrc, parentFileReferenceNumberDst, 16, 16);
                }

                return parentFileReferenceNumber;
            }
            set 
            {
                fixed (byte* parentFileReferenceNumberDst = _parentFileReferenceNumber)
                fixed (byte* parentFileReferenceNumberSrc = value)
                {
                    Buffer.MemoryCopy(parentFileReferenceNumberSrc, parentFileReferenceNumberDst, 16, 16);
                }
            }
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
            get
            {
                fixed (char* fileNamePtr = _fileName)
                {
                    return new string(fileNamePtr, 0, 1);
                }
            }
            set
            {
                fixed (char* fileNamePtr = _fileName)
                {
                    fileNamePtr[0] = value[0];
                }
            }
        }
    }
}