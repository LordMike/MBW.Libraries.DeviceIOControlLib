using System.Runtime.InteropServices;

namespace DeviceIOControlLib.Objects.Disk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_GEOMETRY
    {
        public long Cylinders;
        public MEDIA_TYPE MediaType;
        public int TracksPerCylinder;
        public int SectorsPerTrack;
        public int BytesPerSector;

        public long DiskSize
        {
            get { return Cylinders * TracksPerCylinder * SectorsPerTrack * BytesPerSector; }
        }
    }
}