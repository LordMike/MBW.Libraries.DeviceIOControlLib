using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
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
}