using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Usn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USN
    {
        public long Usn;

        public static implicit operator USN(long usn)
        {
            return new USN { Usn = usn };
        }

        public static implicit operator long(USN usn)
        {
            return usn.Usn;
        }
    }
}