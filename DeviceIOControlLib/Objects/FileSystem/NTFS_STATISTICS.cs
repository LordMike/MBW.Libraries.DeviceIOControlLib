using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
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
}