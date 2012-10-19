using System;
using System.Runtime.InteropServices;

namespace DeviceIOControlLib
{
    public enum PartitionStyle
    {
        PARTITION_STYLE_MBR = 0,
        PARTITION_STYLE_GPT = 1,
        PARTITION_STYLE_RAW = 2
    }

    [Flags]
    public enum EFIPartitionAttributes : ulong
    {
        GPT_ATTRIBUTE_PLATFORM_REQUIRED = 0x0000000000000001,
        LegacyBIOSBootable = 0x0000000000000004,
        GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER = 0x8000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_HIDDEN = 0x4000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_SHADOW_COPY = 0x2000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_READ_ONLY = 0x1000000000000000
    }

    public enum MEDIA_TYPE : uint
    {
        Unknown,
        F5_1Pt2_512,
        F3_1Pt44_512,
        F3_2Pt88_512,
        F3_20Pt8_512,
        F3_720_512,
        F5_360_512,
        F5_320_512,
        F5_320_1024,
        F5_180_512,
        F5_160_512,
        RemovableMedia,
        FixedMedia,
        F3_120M_512,
        F3_640_512,
        F5_640_512,
        F5_720_512,
        F3_1Pt2_512,
        F3_1Pt23_1024,
        F5_1Pt23_1024,
        F3_128Mb_512,
        F3_230Mb_512,
        F8_256_128,
        F3_200Mb_512,
        F3_240M_512,
        F3_32M_512
    }

    public enum DISK_CACHE_RETENTION_PRIORITY : uint
    {
        EqualPriority=0,
        KeepPrefetchedData=1,
        KeepReadData=2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_GEOMETRY
    {
        public long Cylinders;
        public MEDIA_TYPE MediaType;
        public int TracksPerCylinder;
        public int SectorsPerTrack;
        public int BytesPerSector;

        public long DiskSize
        {
            get { return Cylinders * TracksPerCylinder * SectorsPerTrack * BytesPerSector; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_GEOMETRY_EX
    {
        public DISK_GEOMETRY Geometry;
        public long DiskSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION
    {
        public long StartingOffset;
        public long PartitionLength;
        public int HiddenSectors;
        public int PartitionNumber;
        public byte PartitionType;
        [MarshalAs(UnmanagedType.I1)]
        public bool BootIndicator;
        [MarshalAs(UnmanagedType.I1)]
        public bool RecognizedPartition;
        [MarshalAs(UnmanagedType.I1)]
        public bool RewritePartition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION_EX
    {
        [MarshalAs(UnmanagedType.U4)]
        public PartitionStyle PartitionStyle;
        public long StartingOffset;
        public long PartitionLength;
        public int PartitionNumber;
        public bool RewritePartition;
        public PARTITION_INFORMATION_UNION DriveLayoutInformaiton;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PARTITION_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public PARTITION_INFORMATION_GPT Gpt;
        [FieldOffset(0)]
        public PARTITION_INFORMATION_MBR Mbr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION_MBR
    {
        public byte PartitionType;
        [MarshalAs(UnmanagedType.U1)]
        public bool BootIndicator;
        [MarshalAs(UnmanagedType.U1)]
        public bool RecognizedPartition;
        public uint HiddenSectors;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct PARTITION_INFORMATION_GPT
    {
        public Guid PartitionType;
        public Guid PartitionId;
        [MarshalAs(UnmanagedType.U8)]
        public EFIPartitionAttributes Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GET_LENGTH_INFORMATION
    {
        [MarshalAs(UnmanagedType.I8)]
        public long Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DRIVE_LAYOUT_INFORMATION_INTERNAL
    {
        public int PartitionCount;
        public uint Signature;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 128)]
        public PARTITION_INFORMATION[] PartitionEntry;
    }

    public struct DRIVE_LAYOUT_INFORMATION
    {
        public int PartitionCount;
        public uint Signature;
        public PARTITION_INFORMATION[] PartitionEntry;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DRIVE_LAYOUT_INFORMATION_EX_INTERNAL
    {
        [MarshalAs(UnmanagedType.U4)]
        public PartitionStyle PartitionStyle;
        public int PartitionCount;
        public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformaiton;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 128)]
        public PARTITION_INFORMATION_EX[] PartitionEntry;
    }

    public struct DRIVE_LAYOUT_INFORMATION_EX
    {
        public PartitionStyle PartitionStyle;
        public int PartitionCount;
        public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformaiton;
        public PARTITION_INFORMATION_EX[] PartitionEntry;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DRIVE_LAYOUT_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_MBR Mbr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DRIVE_LAYOUT_INFORMATION_MBR
    {
        public ulong Signature { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DRIVE_LAYOUT_INFORMATION_GPT
    {
        public Guid DiskId;
        public long StartingUsableOffset;
        public long UsableLength;
        public ulong MaxPartitionCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_CACHE_INFORMATION
    {
        public bool ParametersSavable;
        public bool ReadCacheEnabled;
        public bool WriteCacheEnabled;
        public DISK_CACHE_RETENTION_PRIORITY ReadRetentionPriority;
        public DISK_CACHE_RETENTION_PRIORITY WriteRetentionPriority;
        public short DisablePrefetchTransferLength;
        public bool PrefetchScalar;

        public DISK_CACHE_INFORMATION_UNION DiskCacheInformationUnion { get; set; }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DISK_CACHE_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public SCALAR_PREFETCH ScalarPrefetch;
        [FieldOffset(0)]
        public BLOCK_PREFETCH BlockPrefetch;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCALAR_PREFETCH
    {
        public short Minimum;
        public short Maximum;
        public short MaximumBlocks;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLOCK_PREFETCH
    {
        public short Minimum;
        public short Maximum;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GET_DISK_ATTRIBUTES
    {
        public int Version;
        public int Reserved1;
        public long Attributes;
    }
}