using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib
{
    public class DeviceIOControlWrapper
    {
        [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
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
        //DiskSmartGetVersion
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

        //FsctlRequestOplockLevel1
        //FsctlRequestOplockLevel2
        //FsctlRequestBatchOplock
        //FsctlOplockBreakAcknowledge
        //FsctlOpBatchAckClosePending
        //FsctlOplockBreakNotify
        //FsctlLockVolume
        //FsctlUnlockVolume
        //FsctlDismountVolume
        //FsctlIsVolumeMounted
        //FsctlIsPathnameValid
        //FsctlMarkVolumeDirty
        //FsctlQueryRetrievalPointers
        //FsctlGetCompression
        //FsctlSetCompression
        //FsctlMarkAsSystemHive
        //FsctlOplockBreakAckNo2
        //FsctlInvalidateVolumes
        //FsctlQueryFatBpb
        //FsctlRequestFilterOplock
        //FsctlFileSystemGetStatistics
        //FsctlGetNtfsVolumeData
        //FsctlGetNtfsFileRecord
        //FsctlGetVolumeBitmap
        //FsctlGetRetrievalPointers
        //FsctlMoveFile
        //FsctlIsVolumeDirty
        //FsctlGetHfsInformation
        //FsctlAllowExtendedDasdIo
        //FsctlReadPropertyData
        //FsctlWritePropertyData
        //FsctlFindFilesBySid
        //FsctlDumpPropertyData
        //FsctlSetObjectId
        //FsctlGetObjectId
        //FsctlDeleteObjectId
        //FsctlSetReparsePoint
        //FsctlGetReparsePoint
        //FsctlDeleteReparsePoint
        //FsctlEnumUsnData
        //FsctlSecurityIdCheck
        //FsctlReadUsnJournal
        //FsctlSetObjectIdExtended
        //FsctlCreateOrGetObjectId
        //FsctlSetSparse
        //FsctlSetZeroData
        //FsctlQueryAllocatedRanges
        //FsctlEnableUpgrade
        //FsctlSetEncryption
        //FsctlEncryptionFsctlIo
        //FsctlWriteRawEncrypted
        //FsctlReadRawEncrypted
        //FsctlCreateUsnJournal
        //FsctlReadFileUsnData
        //FsctlWriteUsnCloseRecord
        //FsctlExtendVolume
        //FsctlQueryUsnJournal
        //FsctlDeleteUsnJournal
        //FsctlMarkHandle
        //FsctlSisCopyFile
        //FsctlSisLinkFiles
        //FsctlHsmMsg
        //FsctlNssControl
        //FsctlHsmData
        //FsctlRecallFile
        //FsctlNssRcontrol

        #endregion

        #region VIDEO

        //VideoQuerySupportedBrightness
        //VideoQueryDisplayBrightness
        //VideoSetDisplayBrightness

        #endregion

        /// <summary>
        /// Invoke DeviceIOControl with no input or output.
        /// </summary>
        /// <returns>Success</returns>
        public static bool InvokeIoControl(SafeFileHandle handle, IOControlCode controlCode)
        {
            uint returnedBytes = 0;

            bool success = DeviceIoControl(handle, controlCode, null, 0, null, 0, ref returnedBytes, IntPtr.Zero);

            return success;
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
    }
}