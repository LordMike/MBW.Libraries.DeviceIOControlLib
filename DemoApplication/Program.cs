using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DeviceIOControlLib;
using Microsoft.Win32.SafeHandles;

namespace DemoApplication
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
           string lpFileName,
           [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
           [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
           [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        static void Main()
        {
            // Read disk sector size
            SafeFileHandle hddHandle = CreateFile(@"\\.\PhysicalDrive0", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            DeviceIOControlWrapper hddDeviceIo = new DeviceIOControlWrapper(hddHandle);

            DISK_GEOMETRY_EX info = hddDeviceIo.DiskGetDriveGeometryEx();

            Console.WriteLine("Sector size: " + info.Geometry.BytesPerSector);

            // Open and close CD Rom tray
            SafeFileHandle cdTrayHandle = CreateFile(@"\\.\CdRom0", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            DeviceIOControlWrapper cdTrayDeviceIo = new DeviceIOControlWrapper(cdTrayHandle);

            // Open tray
            cdTrayDeviceIo.StorageEjectMedia();

            Thread.Sleep(TimeSpan.FromSeconds(10));

            // Close tray
            cdTrayDeviceIo.StorageLoadMedia();
        }
    }
}
