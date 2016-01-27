using System;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.Volume;
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
    }
}