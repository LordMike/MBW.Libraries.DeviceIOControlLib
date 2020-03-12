using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.Storage;
using DeviceIOControlLib.Utilities;
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

        public STORAGE_DEVICE_DESCRIPTOR_PARSED StorageGetDeviceProperty()
        {
            STORAGE_PROPERTY_QUERY query = new STORAGE_PROPERTY_QUERY();
            query.QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery;
            query.PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty;

            byte[] res = DeviceIoControlHelper.InvokeIoControlUnknownSize(Handle, IOControlCode.StorageQueryProperty, query);
            STORAGE_DEVICE_DESCRIPTOR descriptor = Utils.ByteArrayToStruct<STORAGE_DEVICE_DESCRIPTOR>(res, 0);

            STORAGE_DEVICE_DESCRIPTOR_PARSED returnValue = new STORAGE_DEVICE_DESCRIPTOR_PARSED();

            returnValue.Version = descriptor.Version;
            returnValue.Size = descriptor.Size;
            returnValue.DeviceType = descriptor.DeviceType;
            returnValue.DeviceTypeModifier = descriptor.DeviceTypeModifier;
            returnValue.RemovableMedia = descriptor.RemovableMedia;
            returnValue.CommandQueueing = descriptor.CommandQueueing;
            returnValue.VendorIdOffset = descriptor.VendorIdOffset;
            returnValue.ProductIdOffset = descriptor.ProductIdOffset;
            returnValue.ProductRevisionOffset = descriptor.ProductRevisionOffset;
            returnValue.SerialNumberOffset = descriptor.SerialNumberOffset;
            returnValue.BusType = descriptor.BusType;
            returnValue.RawPropertiesLength = descriptor.RawPropertiesLength;
            returnValue.RawDeviceProperties = descriptor.RawDeviceProperties;

            if (descriptor.SerialNumberOffset > 0)
                returnValue.SerialNumber = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.SerialNumberOffset);

            if (descriptor.VendorIdOffset > 0)
                returnValue.VendorId = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.VendorIdOffset);

            if (descriptor.ProductIdOffset > 0)
                returnValue.ProductId = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.ProductIdOffset);

            if (descriptor.ProductRevisionOffset > 0)
                returnValue.ProductRevision = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.ProductRevisionOffset);

            return returnValue;
        }

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