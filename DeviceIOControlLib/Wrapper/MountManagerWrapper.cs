using System;
using System.Collections.Generic;
using System.Text;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.MountManager;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public class MountManagerWrapper : DeviceIoWrapperBase
    {
        public MountManagerWrapper(SafeFileHandle handle, bool ownsHandle = false)
            : base(handle, ownsHandle)
        {
        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/hardware/ff560474(v=vs.85).aspx" />
        /// </summary>
        public List<MountPoint> MountQueryPoints()
        {
            MOUNTMGR_MOUNT_POINT input = new MOUNTMGR_MOUNT_POINT();

            // Fetch in increments of 32 bytes, as one extent (the most common case) is one extent pr. volume.
            byte[] data = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.MountmgrQueryPoints, input, 32);

            uint mountPoints = BitConverter.ToUInt32(data, sizeof(uint));
            uint sizeOfMountPointStruct = MarshalHelper.SizeOf<MOUNTMGR_MOUNT_POINT>();

            List<MountPoint> result = new List<MountPoint>();

            using (UnmanagedMemory mem = new UnmanagedMemory(data))
            {
                IntPtr offset = new IntPtr(mem.Handle.ToInt64() + sizeof(uint) * 2);

                for (int i = 0; i < mountPoints; i++)
                {
                    MOUNTMGR_MOUNT_POINT strct = offset.ToStructure<MOUNTMGR_MOUNT_POINT>();

                    MountPoint point = new MountPoint();

                    point.SymbolicLinkName = Encoding.Unicode.GetString(data, (int)strct.SymbolicLinkNameOffset, strct.SymbolicLinkNameLength);
                    point.DeviceName = Encoding.Unicode.GetString(data, (int)strct.DeviceNameOffset, strct.DeviceNameLength);

                    byte[] idBytes = new byte[strct.UniqueIdLength];
                    Array.Copy(data, (int)strct.UniqueIdOffset, idBytes, 0, idBytes.Length);
                    point.UniqueId = idBytes;

                    result.Add(point);

                    // Advance one
                    offset = new IntPtr(offset.ToInt64() + sizeOfMountPointStruct);
                }
            }

            return result;
        }
    }
}