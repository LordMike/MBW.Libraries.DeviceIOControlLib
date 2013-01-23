using System;

namespace DeviceIOControlLib
{
    [Flags]
    public enum IOFileDevice : uint
    {
        Beep = 0x00000001,
        CDRom = 0x00000002,
        CDRomFileSytem = 0x00000003,
        Controller = 0x00000004,
        Datalink = 0x00000005,
        Dfs = 0x00000006,
        Disk = 0x00000007,
        DiskFileSystem = 0x00000008,
        FileSystem = 0x00000009,
        InPortPort = 0x0000000a,
        Keyboard = 0x0000000b,
        Mailslot = 0x0000000c,
        MidiIn = 0x0000000d,
        MidiOut = 0x0000000e,
        Mouse = 0x0000000f,
        MultiUncProvider = 0x00000010,
        NamedPipe = 0x00000011,
        Network = 0x00000012,
        NetworkBrowser = 0x00000013,
        NetworkFileSystem = 0x00000014,
        Null = 0x00000015,
        ParallelPort = 0x00000016,
        PhysicalNetcard = 0x00000017,
        Printer = 0x00000018,
        Scanner = 0x00000019,
        SerialMousePort = 0x0000001a,
        SerialPort = 0x0000001b,
        Screen = 0x0000001c,
        Sound = 0x0000001d,
        Streams = 0x0000001e,
        Tape = 0x0000001f,
        TapeFileSystem = 0x00000020,
        Transport = 0x00000021,
        Unknown = 0x00000022,
        Video = 0x00000023,
        VirtualDisk = 0x00000024,
        WaveIn = 0x00000025,
        WaveOut = 0x00000026,
        Port8042 = 0x00000027,
        NetworkRedirector = 0x00000028,
        Battery = 0x00000029,
        BusExtender = 0x0000002a,
        Modem = 0x0000002b,
        Vdm = 0x0000002c,
        MassStorage = 0x0000002d,
        Smb = 0x0000002e,
        Ks = 0x0000002f,
        Changer = 0x00000030,
        Smartcard = 0x00000031,
        Acpi = 0x00000032,
        Dvd = 0x00000033,
        FullscreenVideo = 0x00000034,
        DfsFileSystem = 0x00000035,
        DfsVolume = 0x00000036,
        Serenum = 0x00000037,
        Termsrv = 0x00000038,
        Ksec = 0x00000039,
        // From Windows Driver Kit 7
        Fips = 0x0000003A,
        Infiniband = 0x0000003B,
        Vmbus = 0x0000003E,
        CryptProvider = 0x0000003F,
        Wpd = 0x00000040,
        Bluetooth = 0x00000041,
        MtComposite = 0x00000042,
        MtTransport = 0x00000043,
        Biometric = 0x00000044,
        Pmi = 0x00000045,
        Volume = 0x00000056
    }

    [Flags]
    public enum IOMethod : uint
    {
        Buffered = 0,
        InDirect = 1,
        OutDirect = 2,
        Neither = 3
    }

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

    /// <summary>
    /// File attributes are metadata values stored by the file system on disk and are used by the system and are available to developers via various file I/O APIs.
    /// </summary>
    [Flags]
    [CLSCompliant(false)]
    public enum FileAttributes : uint
    {
        /// <summary>
        /// A file that is read-only. Applications can read the file, but cannot write to it or delete it. This attribute is not honored on directories. For more information, see "You cannot view or change the Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, or in Windows Vista".
        /// </summary>
        Readonly = 0x00000001,

        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x00000002,

        /// <summary>
        /// A file or directory that the operating system uses a part of, or uses exclusively.
        /// </summary>
        System = 0x00000004,

        /// <summary>
        /// The handle that identifies a directory.
        /// </summary>
        Directory = 0x00000010,

        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute to mark files for backup or removal.
        /// </summary>
        Archive = 0x00000020,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Device = 0x00000040,

        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone.
        /// </summary>
        Normal = 0x00000080,

        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
        /// </summary>
        Temporary = 0x00000100,

        /// <summary>
        /// A file that is a sparse file.
        /// </summary>
        SparseFile = 0x00000200,

        /// <summary>
        /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
        /// </summary>
        ReparsePoint = 0x00000400,

        /// <summary>
        /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory, compression is the default for newly created files and subdirectories.
        /// </summary>
        Compressed = 0x00000800,

        /// <summary>
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
        /// </summary>
        Offline = 0x00001000,

        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service.
        /// </summary>
        NotContentIndexed = 0x00002000,

        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
        /// </summary>
        Encrypted = 0x00004000,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Virtual = 0x00010000
    }

}