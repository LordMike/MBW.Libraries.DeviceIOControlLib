using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RETRIEVAL_POINTER_BASE
    {
        public ulong FileAreaOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTING_LCN_INPUT_BUFFER
    {
        public ulong StartingLcn;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VOLUME_BITMAP_BUFFER
    {
        public ulong StartingLcn;
        public ulong BitmapSize;

        public BitArray Buffer;
    }

    public class FileSystemStats
    {
        public FILESYSTEM_STATISTICS Stats { get; set; }
        public IFSStats FSStats { get; set; }
    }

    public interface IFSStats { }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILESYSTEM_STATISTICS
    {
        public FILESYSTEM_STATISTICS_TYPE FileSystemType;
        public ushort Version;
        public uint SizeOfCompleteStructure;
        public uint UserFileReads;
        public uint UserFileReadBytes;
        public uint UserDiskReads;
        public uint UserFileWrites;
        public uint UserFileWriteBytes;
        public uint UserDiskWrites;
        public uint MetaDataReads;
        public uint MetaDataReadBytes;
        public uint MetaDataDiskReads;
        public uint MetaDataWrites;
        public uint MetaDataWriteBytes;
        public uint MetaDataDiskWrites;
    }

    public enum FILESYSTEM_STATISTICS_TYPE : ushort
    {
        FILESYSTEM_STATISTICS_TYPE_NTFS = 1,
        FILESYSTEM_STATISTICS_TYPE_FAT = 2,
        FILESYSTEM_STATISTICS_TYPE_EXFAT = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_STATISTICS : IFSStats
    {
        public uint LogFileFullExceptions;
        public uint OtherExceptions;
        public uint MftReads;
        public uint MftReadBytes;
        public uint MftWrites;
        public uint MftWriteBytes;
        public NTFS_STATISTICS_WritesUserLevel MftWritesUserLevel;
        public ushort MftWritesFlushForLogFileFull;
        public ushort MftWritesLazyWriter;
        public ushort MftWritesUserRequest;
        public uint Mft2Writes;
        public uint Mft2WriteBytes;
        public NTFS_STATISTICS_WritesUserLevel Mft2WritesUserLevel;
        public ushort Mft2WritesFlushForLogFileFull;
        public ushort Mft2WritesLazyWriter;
        public ushort Mft2WritesUserRequest;
        public uint RootIndexReads;
        public uint RootIndexReadBytes;
        public uint RootIndexWrites;
        public uint RootIndexWriteBytes;
        public uint BitmapReads;
        public uint BitmapReadBytes;
        public uint BitmapWrites;
        public uint BitmapWriteBytes;
        public ushort BitmapWritesFlushForLogFileFull;
        public ushort BitmapWritesLazyWriter;
        public ushort BitmapWritesUserRequest;
        public NTFS_STATISTICS_WritesUserLevel BitmapWritesUserLevel;
        public uint MftBitmapReads;
        public uint MftBitmapReadBytes;
        public uint MftBitmapWrites;
        public uint MftBitmapWriteBytes;
        public ushort MftBitmapWritesFlushForLogFileFull;
        public ushort MftBitmapWritesLazyWriter;
        public ushort MftBitmapWritesUserRequest;
        public NTFS_STATISTICS_WritesUserLevel MftBitmapWritesUserLevel;
        public uint UserIndexReads;
        public uint UserIndexReadBytes;
        public uint UserIndexWrites;
        public uint UserIndexWriteBytes;
        public uint LogFileReads;
        public uint LogFileReadBytes;
        public uint LogFileWrites;
        public uint LogFileWriteBytes;
        public NTFS_STATISTICS_Allocate Allocate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_STATISTICS_WritesUserLevel
    {
        public ushort Write;
        public ushort Create;
        public ushort SetInfo;
        public ushort Flush;
    }

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

    [StructLayout(LayoutKind.Sequential)]
    public struct EXFAT_STATISTICS : IFSStats
    {
        public uint CreateHits;
        public uint SuccessfulCreates;
        public uint FailedCreates;
        public uint NonCachedReads;
        public uint NonCachedReadBytes;
        public uint NonCachedWrites;
        public uint NonCachedWriteBytes;
        public uint NonCachedDiskReads;
        public uint NonCachedDiskWrites;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FAT_STATISTICS : IFSStats
    {
        public uint CreateHits;
        public uint SuccessfulCreates;
        public uint FailedCreates;
        public uint NonCachedReads;
        public uint NonCachedReadBytes;
        public uint NonCachedWrites;
        public uint NonCachedWriteBytes;
        public uint NonCachedDiskReads;
        public uint NonCachedDiskWrites;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_VOLUME_DATA_BUFFER
    {
        public ulong VolumeSerialNumber;
        public ulong NumberSectors;
        public ulong TotalClusters;
        public ulong FreeClusters;
        public ulong TotalReserved;
        public uint BytesPerSector;
        public uint BytesPerCluster;
        public uint BytesPerFileRecordSegment;
        public uint ClustersPerFileRecordSegment;
        public ulong MftValidDataLength;
        public ulong MftStartLcn;
        public ulong Mft2StartLcn;
        public ulong MftZoneStart;
        public ulong MftZoneEnd;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MFT_ENUM_DATA_V0
    {
        public ulong StartFileReferenceNumber;
        public long LowUsn;
        public long HighUsn;
    }

    public interface IUSN_RECORD
    {
        uint RecordLength { get; set; }
        ushort MajorVersion { get; set; }
        ushort MinorVersion { get; set; }
        long Usn { get; set; }
        ulong TimeStamp { get; set; }
        USN_REASON Reason { get; set; }
        USN_SOURCE_INFO SourceInfo { get; set; }
        uint SecurityId { get; set; }
        FileAttributes FileAttributes { get; set; }
        ushort FileNameLength { get; set; }
        ushort FileNameOffset { get; set; }
        string FileName { get; set; }
    }

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
        private USN_REASON _reason;
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
        public long Usn
        {
            get { return _usn; }
            set { _usn = value; }
        }
        public ulong TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        public USN_REASON Reason
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct USN_RECORD_V3 : IUSN_RECORD
    {
        internal uint _recordLength;
        internal ushort _majorVersion;
        internal ushort _minorVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] _fileReferenceNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] _parentFileReferenceNumber;
        public long _usn;
        public ulong _timeStamp;
        public USN_REASON _reason;
        public USN_SOURCE_INFO _sourceInfo;
        public uint _securityId;
        public FileAttributes _fileAttributes;
        public ushort _fileNameLength;
        public ushort _fileNameOffset;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
        public string _fileName;

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
            get { return _fileReferenceNumber; }
            set { _fileReferenceNumber = value; }
        }
        public byte[] ParentFileReferenceNumber
        {
            get { return _parentFileReferenceNumber; }
            set { _parentFileReferenceNumber = value; }
        }
        public long Usn
        {
            get { return _usn; }
            set { _usn = value; }
        }
        public ulong TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        public USN_REASON Reason
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

    [Flags]
    public enum USN_SOURCE_INFO : uint
    {
        /// <summary>
        /// The operation adds a private data stream to a file or directory.
        /// An example might be a virus detector adding checksum information. As the virus detector modifies the item, the system generates USN records. USN_SOURCE_AUXILIARY_DATA indicates that the modifications did not change the application data.
        /// </summary>
        USN_SOURCE_AUXILIARY_DATA = 0x00000002,

        /// <summary>
        /// The operation provides information about a change to the file or directory made by the operating system.
        /// A typical use is when the Remote Storage system moves data from external to local storage. Remote Storage is the hierarchical storage management software. Such a move usually at a minimum adds the USN_REASON_DATA_OVERWRITE flag to a USN record. However, the data has not changed from the user's point of view. By noting USN_SOURCE_DATA_MANAGEMENT in the SourceInfo member, you can determine that although a write operation is performed on the item, data has not changed.
        /// </summary>
        USN_SOURCE_DATA_MANAGEMENT = 0x00000001,

        /// <summary>
        /// The operation is modifying a file to match the contents of the same file which exists in another member of the replica set.
        /// </summary>
        USN_SOURCE_REPLICATION_MANAGEMENT = 0x00000004
    }

    [Flags]
    public enum USN_REASON : uint
    {
        /// <summary>
        /// A user has either changed one or more file or directory attributes (for example, the read-only, hidden, system, archive, or sparse attribute), or one or more time stamps.
        /// </summary>
        USN_REASON_BASIC_INFO_CHANGE = 0x00008000,

        /// <summary>
        /// The file or directory is closed.
        /// </summary>
        USN_REASON_CLOSE = 0x80000000,

        /// <summary>
        /// The compression state of the file or directory is changed from or to compressed.
        /// </summary>
        USN_REASON_COMPRESSION_CHANGE = 0x00020000,

        /// <summary>
        /// The file or directory is extended (added to).
        /// </summary>
        USN_REASON_DATA_EXTEND = 0x00000002,

        /// <summary>
        /// The data in the file or directory is overwritten.
        /// </summary>
        USN_REASON_DATA_OVERWRITE = 0x00000001,

        /// <summary>
        /// The file or directory is truncated.
        /// </summary>
        USN_REASON_DATA_TRUNCATION = 0x00000004,

        /// <summary>
        /// The user made a change to the extended attributes of a file or directory.
        /// These NTFS file system attributes are not accessible to Windows-based applications.
        /// </summary>
        USN_REASON_EA_CHANGE = 0x00000400,

        /// <summary>
        /// The file or directory is encrypted or decrypted.
        /// </summary>
        USN_REASON_ENCRYPTION_CHANGE = 0x00040000,

        /// <summary>
        /// The file or directory is created for the first time.
        /// </summary>
        USN_REASON_FILE_CREATE = 0x00000100,

        /// <summary>
        /// The file or directory is deleted.
        /// </summary>
        USN_REASON_FILE_DELETE = 0x00000200,

        /// <summary>
        /// An NTFS file system hard link is added to or removed from the file or directory.
        /// An NTFS file system hard link, similar to a POSIX hard link, is one of several directory entries that see the same file or directory.
        /// </summary>
        USN_REASON_HARD_LINK_CHANGE = 0x00010000,

        /// <summary>
        /// A user changes the FILE_ATTRIBUTE_NOT_CONTENT_INDEXED attribute.
        /// That is, the user changes the file or directory from one where content can be indexed to one where content cannot be indexed, or vice versa. Content indexing permits rapid searching of data by building a database of selected content.
        /// </summary>
        USN_REASON_INDEXABLE_CHANGE = 0x00004000,

        /// <summary>
        /// The one or more named data streams for a file are extended (added to).
        /// </summary>
        USN_REASON_NAMED_DATA_EXTEND = 0x00000020,

        /// <summary>
        /// The data in one or more named data streams for a file is overwritten.
        /// </summary>
        USN_REASON_NAMED_DATA_OVERWRITE = 0x00000010,

        /// <summary>
        /// The one or more named data streams for a file is truncated.
        /// </summary>
        USN_REASON_NAMED_DATA_TRUNCATION = 0x00000040,

        /// <summary>
        /// The object identifier of a file or directory is changed.
        /// </summary>
        USN_REASON_OBJECT_ID_CHANGE = 0x00080000,

        /// <summary>
        /// A file or directory is renamed, and the file name in the USN_RECORD_V2 structure is the new name.
        /// </summary>
        USN_REASON_RENAME_NEW_NAME = 0x00002000,

        /// <summary>
        /// The file or directory is renamed, and the file name in the USN_RECORD_V2 structure is the previous name.
        /// </summary>
        USN_REASON_RENAME_OLD_NAME = 0x00001000,

        /// <summary>
        /// The reparse point that is contained in a file or directory is changed, or a reparse point is added to or deleted from a file or directory.
        /// </summary>
        USN_REASON_REPARSE_POINT_CHANGE = 0x00100000,

        /// <summary>
        /// A change is made in the access rights to a file or directory.
        /// </summary>
        USN_REASON_SECURITY_CHANGE = 0x00000800,

        /// <summary>
        /// A named stream is added to or removed from a file, or a named stream is renamed.
        /// </summary>
        USN_REASON_STREAM_CHANGE = 0x00200000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_FILE_RECORD_INPUT_BUFFER
    {
        public ulong FileReferenceNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NTFS_FILE_RECORD_OUTPUT_BUFFER
    {
        public ulong FileReferenceNumber;
        public uint FileRecordLength;
        public byte[] FileRecordBuffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTING_VCN_INPUT_BUFFER
    {
        public ulong StartingVcn;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RETRIEVAL_POINTERS_BUFFER
    {
        public ulong ExtentCount;
        public ulong StartingVcn;
        public RETRIEVAL_POINTERS_EXTENT[] Extents;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RETRIEVAL_POINTERS_EXTENT
    {
        public ulong NextVcn;
        public ulong Lcn;
    }

    public class FileExtentInfo
    {
        /// <summary>
        /// Position in file
        /// </summary>
        public ulong Vcn { get; set; }

        /// <summary>
        /// Position on disk
        /// </summary>
        public ulong Lcn { get; set; }

        /// <summary>
        /// Size in # of clusters
        /// </summary>
        public ulong Size { get; set; }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct MOVE_FILE_DATA
    {
        public IntPtr FileHandle;
        public ulong StartingVcn;
        public ulong StartingLcn;
        public uint ClusterCount;
    }
}
