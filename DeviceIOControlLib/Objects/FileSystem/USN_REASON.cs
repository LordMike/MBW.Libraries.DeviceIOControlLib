using System;

namespace DeviceIOControlLib.Objects.FileSystem
{
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
}