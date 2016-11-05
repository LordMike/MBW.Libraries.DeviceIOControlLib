using System;
using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public class DiskDeviceWrapper : DeviceIoWrapperBase
    {
        public DiskDeviceWrapper(SafeFileHandle handle, bool ownsHandle = false)
            : base(handle, ownsHandle)
        {
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365169(v=vs.85).aspx"/></summary>
        public DISK_GEOMETRY DiskGetDriveGeometry()
        {
            return DeviceIoControlHelper.InvokeIoControl<DISK_GEOMETRY>(Handle, IOControlCode.DiskGetDriveGeometry);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365171(v=vs.85).aspx"/></summary>
        public DISK_GEOMETRY_EX DiskGetDriveGeometryEx()
        {
            byte[] data = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.DiskGetDriveGeometryEx, 256);

            DISK_GEOMETRY_EX res;

            using (UnmanagedMemory mem = new UnmanagedMemory(data))
            {
                res.Geometry = mem.Handle.ToStructure<DISK_GEOMETRY>();
                res.DiskSize = BitConverter.ToInt64(data, (int)MarshalHelper.SizeOf<DISK_GEOMETRY>());

                IntPtr tmpPtr = new IntPtr(mem.Handle.ToInt64() + MarshalHelper.SizeOf<DISK_GEOMETRY>() + sizeof(long));
                res.PartitionInformation = tmpPtr.ToStructure<DISK_PARTITION_INFO>();

                tmpPtr = new IntPtr(tmpPtr.ToInt64() + res.PartitionInformation.SizeOfPartitionInfo);
                res.DiskInt13Info = tmpPtr.ToStructure<DISK_EX_INT13_INFO>();
            }

            return res;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365179(v=vs.85).aspx"/></summary>
        public PARTITION_INFORMATION DiskGetPartitionInfo()
        {
            return DeviceIoControlHelper.InvokeIoControl<PARTITION_INFORMATION>(Handle, IOControlCode.DiskGetPartitionInfo);
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365180(v=vs.85).aspx"/></summary>
        public PARTITION_INFORMATION_EX DiskGetPartitionInfoEx()
        {
            return DeviceIoControlHelper.InvokeIoControl<PARTITION_INFORMATION_EX>(Handle, IOControlCode.DiskGetPartitionInfoEx);
        }

        //DiskSetPartitionInfo
        //DiskSetPartitionInfoEx

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365173(v=vs.85).aspx"/></summary>
        public DRIVE_LAYOUT_INFORMATION DiskGetDriveLayout()
        {
            DRIVE_LAYOUT_INFORMATION_INTERNAL data = DeviceIoControlHelper.InvokeIoControl<DRIVE_LAYOUT_INFORMATION_INTERNAL>(Handle, IOControlCode.DiskGetDriveLayout);

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
            DRIVE_LAYOUT_INFORMATION_EX_INTERNAL data = DeviceIoControlHelper.InvokeIoControl<DRIVE_LAYOUT_INFORMATION_EX_INTERNAL>(Handle, IOControlCode.DiskGetDriveLayoutEx);

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
            return DeviceIoControlHelper.InvokeIoControl<GETVERSIONINPARAMS>(Handle, IOControlCode.DiskSmartGetVersion);
        }

        //DiskSmartSendDriveCommand
        //DiskSmartRcvDriveData
        //DiskUpdateDriveSize
        //DiskGrowPartition

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365165(v=vs.85).aspx"/></summary>
        public DISK_CACHE_INFORMATION DiskGetCacheInformation()
        {
            return DeviceIoControlHelper.InvokeIoControl<DISK_CACHE_INFORMATION>(Handle, IOControlCode.DiskGetCacheInformation);
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
            return DeviceIoControlHelper.InvokeIoControl<GET_LENGTH_INFORMATION>(Handle, IOControlCode.DiskGetLengthInfo).Length;
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/hh706681(v=vs.85).aspx"/></summary>
        public GET_DISK_ATTRIBUTES DiskGetDiskAttributes()
        {
            return DeviceIoControlHelper.InvokeIoControl<GET_DISK_ATTRIBUTES>(Handle, IOControlCode.DiskGetDiskAttributes);
        }

        //DiskSetDiskAttributes
    }
}