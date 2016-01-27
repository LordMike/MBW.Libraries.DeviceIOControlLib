using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.FileSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct STARTING_VCN_INPUT_BUFFER
    {
        public ulong StartingVcn;
    }
}