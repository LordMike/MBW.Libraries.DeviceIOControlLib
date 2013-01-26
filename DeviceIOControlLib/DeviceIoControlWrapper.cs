using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib
{
    public class DeviceIOControlWrapper
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode IoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [In] object InBuffer,
            uint nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            [In] IntPtr Overlapped
            );
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode ioControlCode,
            byte[] inBuffer,
            uint nInBufferSize,
            byte[] outBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr overlapped
            );

        public SafeFileHandle Handle { get; private set; }

        public DeviceIOControlWrapper(SafeFileHandle handle)
        {
            if (handle.IsInvalid)
                throw new ArgumentException("Handle is invalid");

            Handle = handle;
        }

        #region STORAGE

        //StorageCheckVerify
        //StorageCheckVerify2
        //StorageMediaRemoval

        /// <summary>
        /// Used to f.ex. open/eject CD Rom trays
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa363406(v=vs.85).aspx" />
        /// </summary>
        public bool StorageEjectMedia()
        {
            return InvokeIoControl(Handle, IOControlCode.StorageEjectMedia);
        }

        /// <summary>
        /// Used to f.ex. close CD Rom trays
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa363414(v=vs.85).aspx" />
        /// </summary>
        public bool StorageLoadMedia()
        {
            return InvokeIoControl(Handle, IOControlCode.StorageLoadMedia);
        }

        //StorageLoadMedia2
        //StorageReserve
        //StorageRelease
        //StorageFindNewDevices
        //StorageEjectionControl
        //StorageMcnControl
        //StorageGetMediaTypes
        //StorageGetMediaTypesEx
        //StorageResetBus
        //StorageResetDevice
        //StorageGetDeviceNumber
        //StoragePredictFailure
        //StorageObsoleteResetBus
        //StorageObsoleteResetDevice

        #endregion

        #region DISK

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365169(v=vs.85).aspx"/></summary>
        public DISK_GEOMETRY DiskGetDriveGeometry()
        {
            return InvokeIoControl<DISK_GEOMETRY>(Handle, IOControlCode.DiskGetDriveGeometry);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365171(v=vs.85).aspx"/></summary>
        public DISK_GEOMETRY_EX DiskGetDriveGeometryEx()
        {
            return InvokeIoControl<DISK_GEOMETRY_EX>(Handle, IOControlCode.DiskGetDriveGeometryEx);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365179(v=vs.85).aspx"/></summary>
        public PARTITION_INFORMATION DiskGetPartitionInfo()
        {
            return InvokeIoControl<PARTITION_INFORMATION>(Handle, IOControlCode.DiskGetPartitionInfo);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365180(v=vs.85).aspx"/></summary>
        public PARTITION_INFORMATION_EX DiskGetPartitionInfoEx()
        {
            return InvokeIoControl<PARTITION_INFORMATION_EX>(Handle, IOControlCode.DiskGetPartitionInfoEx);
        }

        //DiskSetPartitionInfo
        //DiskSetPartitionInfoEx

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365173(v=vs.85).aspx"/></summary>
        public DRIVE_LAYOUT_INFORMATION DiskGetDriveLayout()
        {
            DRIVE_LAYOUT_INFORMATION_INTERNAL data = InvokeIoControl<DRIVE_LAYOUT_INFORMATION_INTERNAL>(Handle, IOControlCode.DiskGetDriveLayout);

            DRIVE_LAYOUT_INFORMATION res = new DRIVE_LAYOUT_INFORMATION();

            res.PartitionCount = data.PartitionCount;
            res.Signature = data.Signature;
            res.PartitionEntry = new PARTITION_INFORMATION[res.PartitionCount];

            for (int i = 0; i < res.PartitionCount; i++)
                res.PartitionEntry[i] = data.PartitionEntry[i];

            return res;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365174(v=vs.85).aspx"/></summary>
        public DRIVE_LAYOUT_INFORMATION_EX DiskGetDriveLayoutEx()
        {
            DRIVE_LAYOUT_INFORMATION_EX_INTERNAL data = InvokeIoControl<DRIVE_LAYOUT_INFORMATION_EX_INTERNAL>(Handle, IOControlCode.DiskGetDriveLayoutEx);

            DRIVE_LAYOUT_INFORMATION_EX res = new DRIVE_LAYOUT_INFORMATION_EX();

            res.PartitionStyle = data.PartitionStyle;
            res.PartitionCount = data.PartitionCount;
            res.DriveLayoutInformaiton = data.DriveLayoutInformaiton;
            res.PartitionEntry = new PARTITION_INFORMATION_EX[res.PartitionCount];

            for (int i = 0; i < res.PartitionCount; i++)
                res.PartitionEntry[i] = data.PartitionEntry[i];

            return res;
        }

        //DiskSetDriveLayout
        //DiskSetDriveLayoutEx
        //DiskVerify
        //DiskFormatTracks
        //DiskReassignBlocks
        //DiskPerformance
        //DiskIsWritable
        //DiskLogging
        //DiskFormatTracksEx
        //DiskHistogramStructure
        //DiskHistogramData
        //DiskHistogramReset
        //DiskRequestStructure
        //DiskRequestData
        //DiskControllerNumber

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/hardware/ff566202(v=vs.85).aspx"/></summary>
        public GETVERSIONINPARAMS DiskGetSmartVersion()
        {
            return InvokeIoControl<GETVERSIONINPARAMS>(Handle, IOControlCode.DiskSmartGetVersion);
        }

        //DiskSmartSendDriveCommand
        //DiskSmartRcvDriveData
        //DiskUpdateDriveSize
        //DiskGrowPartition

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365165(v=vs.85).aspx"/></summary>
        public DISK_CACHE_INFORMATION DiskGetCacheInformation()
        {
            return InvokeIoControl<DISK_CACHE_INFORMATION>(Handle, IOControlCode.DiskGetCacheInformation);
        }

        //DiskSetCacheInformation
        //DiskDeleteDriveLayout
        //DiskFormatDrive
        //DiskSenseDevice
        //DiskCheckVerify
        //DiskMediaRemoval
        //DiskEjectMedia
        //DiskLoadMedia
        //DiskReserve
        //DiskRelease
        //DiskFindNewDevices
        //DiskCreateDisk

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365178(v=vs.85).aspx"/></summary>
        public long DiskGetLengthInfo()
        {
            return InvokeIoControl<GET_LENGTH_INFORMATION>(Handle, IOControlCode.DiskGetLengthInfo).Length;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/hh706681(v=vs.85).aspx"/></summary>
        public GET_DISK_ATTRIBUTES DiskGetDiskAttributes()
        {
            return InvokeIoControl<GET_DISK_ATTRIBUTES>(Handle, IOControlCode.DiskGetDiskAttributes);
        }

        //DiskSetDiskAttributes

        #endregion

        #region CHANGER

        //ChangerGetParameters
        //ChangerGetStatus
        //ChangerGetProductData
        //ChangerSetAccess
        //ChangerGetElementStatus
        //ChangerInitializeElementStatus
        //ChangerSetPosition
        //ChangerExchangeMedium
        //ChangerMoveMedium
        //ChangerReinitializeTarget
        //ChangerQueryVolumeTags

        #endregion

        #region FILESYSTEM

        //FsctlAllowExtendedDasdIo
        //FsctlCreateOrGetObjectId
        //FsctlCreateUsnJournal
        //FsctlDeleteObjectId
        //FsctlDeleteReparsePoint
        //FsctlDeleteUsnJournal
        //FsctlDismountVolume
        //FsctlDumpPropertyData
        //FsctlEnableUpgrade
        //FsctlEncryptionFsctlIo

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa364563(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemEnumUsnData()
        {
            ulong nextUsn = 0;
            const int chunkSize = 1 * 1024 * 1024;      // 1 MB chunks

            IntPtr dataPtr = IntPtr.Zero;

            List<IUSN_RECORD> res = new List<IUSN_RECORD>();

            try
            {
                dataPtr = Marshal.AllocHGlobal(chunkSize);

                do
                {
                    MFT_ENUM_DATA_V0 input = new MFT_ENUM_DATA_V0();
                    input.StartFileReferenceNumber = nextUsn;
                    input.LowUsn = long.MinValue;
                    input.HighUsn = long.MaxValue;

                    int errorCode;
                    byte[] data = InvokeIoControl(Handle, IOControlCode.FsctlEnumUsnData, chunkSize, input, out errorCode);

                    if (errorCode != 0)
                        // Exit when theres no more to do
                        break;

                    nextUsn = BitConverter.ToUInt64(data, 0);

                    int dataOffset = 8;

                    while (dataOffset < data.Length)
                    {
                        // Get record length
                        int length = BitConverter.ToInt32(data, dataOffset);
                        ushort majorVersion = BitConverter.ToUInt16(data, dataOffset + sizeof (int));

                        if (length <= 0)
                            // No more records
                            break;

                        // Copy out record subset
                        byte[] bytes = new byte[length];
                        Array.Copy(data, dataOffset, bytes, 0, length);

                        Marshal.Copy(bytes, 0, dataPtr, bytes.Length);

                        switch (majorVersion)
                        {
                            case 2:
                                USN_RECORD_V2 recordv2 = (USN_RECORD_V2)Marshal.PtrToStructure(dataPtr, typeof(USN_RECORD_V2));

                                // Parse string manually, as we cannot rely on the string to be null-terminated.
                                recordv2.FileName = Encoding.Unicode.GetString(bytes, recordv2.FileNameOffset, recordv2.FileNameLength);

                                res.Add(recordv2);

                                break;
                            case 3:
                                USN_RECORD_V3 recordv3 = (USN_RECORD_V3)Marshal.PtrToStructure(dataPtr, typeof(USN_RECORD_V3));

                                // Parse string manually, as we cannot rely on the string to be null-terminated.
                                recordv3.FileName = Encoding.Unicode.GetString(bytes, recordv3.FileNameOffset, recordv3.FileNameLength);

                                res.Add(recordv3);

                                break;
                            default:
                                // Ignore
                                Debugger.Break();
                                break;
                        }

                        // Move to next record
                        dataOffset += length;
                    }

                    // Fetch next chunk
                } while (true);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return res.ToArray();
        }

        //FsctlExtendVolume

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa364565(v=vs.85).aspx"/></summary>
        public FileSystemStats[] FileSystemGetStatistics()
        {
            byte[] data = InvokeIoControlUnknownSize(Handle, IOControlCode.FsctlFileSystemGetStatistics, 512);
            IntPtr dataPtr = IntPtr.Zero;

            FileSystemStats[] res;

            try
            {
                dataPtr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, dataPtr, data.Length);

                IntPtr currentDataPtr = dataPtr;

                FILESYSTEM_STATISTICS firstStats = (FILESYSTEM_STATISTICS)Marshal.PtrToStructure(currentDataPtr, typeof(FILESYSTEM_STATISTICS));

                int fsStatsSize = Marshal.SizeOf(typeof(FILESYSTEM_STATISTICS));

                int elementSize = (int)firstStats.SizeOfCompleteStructure;
                int procCount = data.Length / elementSize;

                res = new FileSystemStats[procCount];

                for (int i = 0; i < procCount; i++)
                {
                    res[i] = new FileSystemStats();
                    res[i].Stats = (FILESYSTEM_STATISTICS)Marshal.PtrToStructure(currentDataPtr, typeof(FILESYSTEM_STATISTICS));

                    switch (res[i].Stats.FileSystemType)
                    {
                        case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_NTFS:
                            NTFS_STATISTICS ntfsStats = (NTFS_STATISTICS)Marshal.PtrToStructure(currentDataPtr + fsStatsSize, typeof(NTFS_STATISTICS));

                            res[i].FSStats = ntfsStats;

                            break;
                        case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_FAT:
                            FAT_STATISTICS fatStats = (FAT_STATISTICS)Marshal.PtrToStructure(currentDataPtr + fsStatsSize, typeof(FAT_STATISTICS));

                            res[i].FSStats = fatStats;
                            break;
                        case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_EXFAT:
                            EXFAT_STATISTICS exFatStats = (EXFAT_STATISTICS)Marshal.PtrToStructure(currentDataPtr + fsStatsSize, typeof(EXFAT_STATISTICS));

                            res[i].FSStats = exFatStats;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    currentDataPtr += elementSize;
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return res;
        }

        //FsctlFindFilesBySid
        //FsctlGetCompression
        //FsctlGetHfsInformation

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/aa364568(v=vs.85).aspx"/></summary>
        public NTFS_FILE_RECORD_OUTPUT_BUFFER FileSystemGetNtfsFileRecord(ulong fileId)
        {
            NTFS_FILE_RECORD_INPUT_BUFFER input = new NTFS_FILE_RECORD_INPUT_BUFFER();
            input.FileReferenceNumber = fileId;

            byte[] data = InvokeIoControlUnknownSize(Handle, IOControlCode.FsctlGetNtfsFileRecord, input, 1024);    // NTFS File records are in 1K chunks

            NTFS_FILE_RECORD_OUTPUT_BUFFER res = new NTFS_FILE_RECORD_OUTPUT_BUFFER();
            res.FileReferenceNumber = BitConverter.ToUInt64(data, 0);
            res.FileRecordLength = BitConverter.ToUInt32(data, 8);

            res.FileRecordBuffer = new byte[res.FileRecordLength];
            Array.Copy(data, 8 + 4, res.FileRecordBuffer, 0, res.FileRecordLength);

            return res;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa364569(v=vs.85).aspx"/></summary>
        public NTFS_VOLUME_DATA_BUFFER FileSystemGetNtfsVolumeData()
        {
            return InvokeIoControl<NTFS_VOLUME_DATA_BUFFER>(Handle, IOControlCode.FsctlGetNtfsVolumeData);
        }

        //FsctlGetObjectId
        //FsctlGetReparsePoint

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/dd405526(v=vs.85).aspx"/></summary>
        public RETRIEVAL_POINTER_BASE FileSystemGetRetrievalPointerBase()
        {
            return InvokeIoControl<RETRIEVAL_POINTER_BASE>(Handle, IOControlCode.FsctlGetRetrievalPointerBase);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/dd405526(v=vs.85).aspx"/></summary>
        /// <remarks>
        ///     Does not correcly handle all cases of this method!. Especially regarding compressed/encrypted and/or sparse files in NTFS.
        ///     Consider yourself warned.
        /// </remarks>
        public FileExtentInfo[] FileSystemGetRetrievalPointers(ulong startingVcn = 0)
        {
            STARTING_VCN_INPUT_BUFFER input = new STARTING_VCN_INPUT_BUFFER();
            input.StartingVcn = startingVcn;

            byte[] data = InvokeIoControlUnknownSize(Handle, IOControlCode.FsctlGetRetrievalPointers, input, 1024);

            RETRIEVAL_POINTERS_BUFFER res = new RETRIEVAL_POINTERS_BUFFER();
            res.ExtentCount = BitConverter.ToUInt32(data, 0);
            res.StartingVcn = BitConverter.ToUInt64(data, sizeof(ulong));

            res.Extents = new RETRIEVAL_POINTERS_EXTENT[res.ExtentCount];

            IntPtr dataPtr = IntPtr.Zero;

            try
            {
                uint singleSize = (uint)Marshal.SizeOf(typeof(RETRIEVAL_POINTERS_EXTENT));
                int extentsSize = (int)(res.ExtentCount * singleSize);
                dataPtr = Marshal.AllocHGlobal(extentsSize);

                Marshal.Copy(data, sizeof(ulong) + sizeof(ulong), dataPtr, extentsSize);

                for (ulong i = 0; i < res.ExtentCount; i++)
                {
                    IntPtr currentPtr = dataPtr + (int)(singleSize * i);
                    res.Extents[i] = (RETRIEVAL_POINTERS_EXTENT)Marshal.PtrToStructure(currentPtr, typeof(RETRIEVAL_POINTERS_EXTENT));
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            FileExtentInfo[] extents = new FileExtentInfo[res.ExtentCount];

            for (ulong i = 0; i < res.ExtentCount; i++)
            {
                ulong startVcn = i == 0
                                 ? res.StartingVcn
                                 : res.Extents[i - 1].NextVcn;

                ulong size = res.Extents[i].NextVcn - startVcn;

                FileExtentInfo extent = new FileExtentInfo();
                extent.Size = size;
                extent.Vcn = startVcn;
                extent.Lcn = res.Extents[i].Lcn;

                extents[i] = extent;
            }

            return extents;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa364573(v=vs.85).aspx"/></summary>
        public VOLUME_BITMAP_BUFFER FileSystemGetVolumeBitmap(ulong startingLcn = 0)
        {
            STARTING_LCN_INPUT_BUFFER startingLcnStruct = new STARTING_LCN_INPUT_BUFFER();
            startingLcnStruct.StartingLcn = startingLcn;

            // Fetch 128 bytes (this includes the size parameter of the VOLUME_BITMAP_BUFFER structure.
            int lastError;
            byte[] data = InvokeIoControl(Handle, IOControlCode.FsctlGetVolumeBitmap, 128, startingLcnStruct, out lastError);

            // Is there more data? (Most often there is).
            if (lastError == 234)
            {
                // Parse length attribute (2'nd 64-bit attribute)
                uint newLength = (uint)BitConverter.ToUInt64(data, 8);

                // Length: Clusters / 8 + 2x 64 bit numbers
                newLength = (uint)Math.Ceiling(newLength / 8d) + 2 * 8;

                data = InvokeIoControl(Handle, IOControlCode.FsctlGetVolumeBitmap, newLength, startingLcnStruct, out lastError);
            }

            // Ensure the last call to InvokeIoControl succeeded.
            if (lastError != 0)
                throw new Win32Exception("Couldn't invoke FileSystemGetVolumeBitmap. LastError: " + Utils.GetWin32ErrorMessage(lastError));

            // Build the VOLUME_BITMAP_BUFFER structure.
            VOLUME_BITMAP_BUFFER res = new VOLUME_BITMAP_BUFFER();

            res.StartingLcn = BitConverter.ToUInt64(data, 0);
            res.BitmapSize = BitConverter.ToUInt64(data, sizeof(UInt64));

            res.Buffer = new BitArray((int)res.BitmapSize);

            for (int i = 0; i < res.Buffer.Length; i++)
            {
                int dataByteIndex = sizeof(UInt64) * 2 + i / 8;
                byte dataByte = data[dataByteIndex];

                int byteIdx = 7 - (i % 8);

                res.Buffer[i] = ((dataByte >> byteIdx) & 1) == 1;
            }

            return res;
        }

        //FsctlHsmData
        //FsctlHsmMsg
        //FsctlInvalidateVolumes
        //FsctlIsPathnameValid
        //FsctlIsVolumeDirty
        //FsctlIsVolumeMounted
        //FsctlLockVolume
        //FsctlMarkAsSystemHive
        //FsctlMarkHandle
        //FsctlMarkVolumeDirty

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/aa364577(v=vs.85).aspx"/></summary>
        public void FileSystemMoveFile(IntPtr fileHandle, ulong startingVcn, ulong startingLcn, uint clusterCount)
        {
            MOVE_FILE_DATA input = new MOVE_FILE_DATA();
            input.FileHandle = fileHandle;
            input.StartingVcn = startingVcn;
            input.StartingLcn = startingLcn;
            input.ClusterCount = clusterCount;

            InvokeIoControl(Handle, IOControlCode.FsctlMoveFile, input);
        }

        //FsctlNssControl
        //FsctlNssRcontrol
        //FsctlOpBatchAckClosePending
        //FsctlOplockBreakAckNo2
        //FsctlOplockBreakAcknowledge
        //FsctlOplockBreakNotify
        //FsctlQueryAllocatedRanges
        //FsctlQueryFatBpb
        //FsctlQueryRetrievalPointers
        //FsctlQueryOnDiskVolumeInfo
        //FsctlQueryUsnJournal
        //FsctlReadFileUsnData
        //FsctlReadPropertyData
        //FsctlReadRawEncrypted
        //FsctlReadUsnJournal
        //FsctlRecallFile
        //FsctlRequestBatchOplock
        //FsctlRequestFilterOplock
        //FsctlRequestOplockLevel1
        //FsctlRequestOplockLevel2
        //FsctlSecurityIdCheck
        //FsctlSetCompression
        //FsctlSetEncryption
        //FsctlSetObjectId
        //FsctlSetObjectIdExtended
        //FsctlSetReparsePoint
        //FsctlSetSparse
        //FsctlSetZeroData
        //FsctlSisCopyFile
        //FsctlSisLinkFiles
        //FsctlUnlockVolume
        //FsctlWritePropertyData
        //FsctlWriteRawEncrypted
        //FsctlWriteUsnCloseRecord

        #endregion

        #region VIDEO

        //VideoQuerySupportedBrightness
        //VideoQueryDisplayBrightness
        //VideoSetDisplayBrightness

        #endregion

        #region VOLUME

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365194(v=vs.85).aspx" />
        /// </summary>
        public VOLUME_DISK_EXTENTS VolumeGetVolumeDiskExtents()
        {
            // Fetch in increments of 32 bytes, as one extent (the most common case) is one extent pr. volume.
            byte[] data = InvokeIoControlUnknownSize(Handle, IOControlCode.VolumeGetVolumeDiskExtents, 32);

            // Build the VOLUME_DISK_EXTENTS structure
            VOLUME_DISK_EXTENTS res = new VOLUME_DISK_EXTENTS();

            res.NumberOfDiskExtents = BitConverter.ToUInt32(data, 0);
            res.Extents = new DISK_EXTENT[res.NumberOfDiskExtents];

            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, dataPtr, data.Length);

                // TODO: This code needs to be tested for disks with more than one extent.
                for (int i = 0; i < res.NumberOfDiskExtents; i++)
                {
                    IntPtr currentDataPtr = dataPtr + 8 + i * Marshal.SizeOf(typeof(DISK_EXTENT));
                    DISK_EXTENT extent = (DISK_EXTENT)Marshal.PtrToStructure(currentDataPtr, typeof(DISK_EXTENT));

                    res.Extents[i] = extent;
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return res;
        }

        #endregion

        /// <summary>
        /// Invoke DeviceIOControl with no input or output.
        /// </summary>
        /// <returns>Success</returns>
        public static bool InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode)
        {
            uint returnedBytes = 0;

            return DeviceIoControl(handle, controlCode, null, 0, null, 0, ref returnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of a byte array.
        /// </summary>
        private static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength)
        {
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of a byte array. Lets the caller handle the errorcode (if any).
        /// </summary>
        private static byte[] InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, out int errorCode)
        {
            uint returnedBytes = 0;

            byte[] output = new byte[outputLength];
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

            errorCode = 0;

            if (!success)
                errorCode = Marshal.GetLastWin32Error();

            return output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with no input, and retrieve the output in the form of an object of type T.
        /// </summary>
        private static T InvokeIoControl<T>(SafeFileHandle handle, IOControlCode controlCode)
        {
            uint returnedBytes = 0;

            object output = default(T);
            uint outputSize = (uint)Marshal.SizeOf(output);
            bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return (T)output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieve the output in the form of an object of type T.
        /// </summary>
        private static T InvokeIoControl<T, V>(SafeFileHandle handle, IOControlCode controlCode, V input)
        {
            uint returnedBytes = 0;

            object output = default(T);
            uint outputSize = (uint)Marshal.SizeOf(output);

            uint inputSize = (uint)Marshal.SizeOf(input);
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputSize, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            return (T)output;
        }

        /// <summary>
        /// Invoke DeviceIOControl with input of type V, and retrieves no output.
        /// </summary>
        private static void InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, V input)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)Marshal.SizeOf(input);
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, null, 0, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }
        }

        /// <summary>
        /// Calls InvokeIoControl with the specified input, returning a byte array. It allows the caller to handle errors.
        /// </summary>
        private static byte[] InvokeIoControl<V>(SafeFileHandle handle, IOControlCode controlCode, uint outputLength, V input, out int errorCode)
        {
            uint returnedBytes = 0;
            uint inputSize = (uint)Marshal.SizeOf(input);

            errorCode = 0;
            byte[] output = new byte[outputLength];

            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                errorCode = Marshal.GetLastWin32Error();
            }

            return output;
        }
        /// <summary>
        /// Repeatedly invokes InvokeIoControl, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        private static byte[] InvokeIoControlUnknownSize(SafeFileHandle handle, IOControlCode controlCode, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint outputLength = increment;

            do
            {
                byte[] output = new byte[outputLength];
                bool success = DeviceIoControl(handle, controlCode, null, 0, output, outputLength, ref returnedBytes, IntPtr.Zero);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == 234)
                    {
                        // More data
                        outputLength += increment;
                        continue;
                    }

                    throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, returnedBytes);

                return res;
            } while (true);
        }

        /// <summary>
        /// Repeatedly invokes InvokeIoControl with the specified input, as long as it gets return code 234 ("More data available") from the method.
        /// </summary>
        private static byte[] InvokeIoControlUnknownSize<V>(SafeFileHandle handle, IOControlCode controlCode, V input, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)Marshal.SizeOf(input);
            uint outputLength = increment;

            do
            {
                byte[] output = new byte[outputLength];
                bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == 234)
                    {
                        // More data
                        outputLength += increment;
                        continue;
                    }

                    throw new Win32Exception("Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
                }

                // Return the result
                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, returnedBytes);

                return res;
            } while (true);
        }
    }
}