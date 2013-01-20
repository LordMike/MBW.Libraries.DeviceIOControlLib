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
}
