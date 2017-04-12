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

            byte[] data = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.MountmgrQueryPoints, input, 512);

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

        /// <summary>
        /// No reference
        /// </summary>
        public List<string> QueryDosVolumePaths(string deviceName)
        {
            int byteCount = Encoding.Unicode.GetByteCount(deviceName);
            byte[] tmp = new byte[sizeof(ushort) + byteCount];

            tmp[0] = (byte)(byteCount & 0xFF);
            tmp[1] = (byte)((byteCount >> 8) & 0xFF);

            Encoding.Unicode.GetBytes(deviceName, 0, deviceName.Length, tmp, 2);

            byte[] data = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.MountmgrQueryDosVolumePaths, tmp, 64, (uint)tmp.Length);

            return new List<string>(Utils.ReadUnicodeStringArray(data, sizeof(uint)));
        }
    }
}