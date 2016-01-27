using DeviceIOControlLib.Objects.Enums;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public class StorageDeviceWrapper : DeviceIoWrapperBase
    {
        public StorageDeviceWrapper(SafeFileHandle handle, bool ownsHandle = false)
            : base(handle, ownsHandle)
        {

        }

        //StorageCheckVerify
        //StorageCheckVerify2
        //StorageMediaRemoval

        /// <summary>
        /// Used to f.ex. open/eject CD Rom trays
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa363406(v=vs.85).aspx" />
        /// </summary>
        public bool StorageEjectMedia()
        {
            return DeviceIoControlHelper.InvokeIoControl(Handle, IOControlCode.StorageEjectMedia);
        }

        /// <summary>
        /// Used to f.ex. close CD Rom trays
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa363414(v=vs.85).aspx" />
        /// </summary>
        public bool StorageLoadMedia()
        {
            return DeviceIoControlHelper.InvokeIoControl(Handle, IOControlCode.StorageLoadMedia);
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

    }
}