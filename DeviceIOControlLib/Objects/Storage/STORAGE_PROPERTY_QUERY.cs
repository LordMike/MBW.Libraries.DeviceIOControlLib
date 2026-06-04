using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Storage
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct STORAGE_PROPERTY_QUERY
    {
        public STORAGE_PROPERTY_ID PropertyId;
        public STORAGE_QUERY_TYPE QueryType;
        public byte* AdditionalParameters;
    }
}