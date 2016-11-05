using System;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.Volume;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public class VolumeDeviceWrapper : DeviceIoWrapperBase
    {
        public VolumeDeviceWrapper(SafeFileHandle handle, bool ownsHandle = false)
            : base(handle, ownsHandle)
        {
        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa365194(v=vs.85).aspx" />
        /// </summary>
        public VOLUME_DISK_EXTENTS VolumeGetVolumeDiskExtents()
        {
            // Fetch in increments of 32 bytes, as one extent (the most common case) is one extent pr. volume.
            byte[] data = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.VolumeGetVolumeDiskExtents, 32);

            // Build the VOLUME_DISK_EXTENTS structure
            VOLUME_DISK_EXTENTS res = new VOLUME_DISK_EXTENTS();

            res.NumberOfDiskExtents = BitConverter.ToUInt32(data, 0);
            res.Extents = new DISK_EXTENT[res.NumberOfDiskExtents];

            using (UnmanagedMemory dataPtr = new UnmanagedMemory(data))
            {
                // TODO: This code needs to be tested for volumes with more than one extent.
                for (int i = 0; i < res.NumberOfDiskExtents; i++)
                {
                    IntPtr currentDataPtr = new IntPtr(dataPtr.Handle.ToInt64() + 8 + i * MarshalHelper.SizeOf<DISK_EXTENT>());
                    DISK_EXTENT extent = currentDataPtr.ToStructure<DISK_EXTENT>();

                    res.Extents[i] = extent;
                }
            }

            return res;
        }
    }
}