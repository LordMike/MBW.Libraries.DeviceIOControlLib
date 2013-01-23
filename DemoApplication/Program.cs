using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DeviceIOControlLib;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;

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

        private static void Main()
        {
            // Read disk sector size
            ExampleDiskIO();

            // Read volume info
            ExampleFileSystemIO();

            // Open and close CD Rom tray
            ExampleCdRomIO();

            // Draw a bitmap image of the disk allocation bitmap
            ExampleBitmap();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void ExampleDiskIO()
        {
            const string drive = @"\\.\PhysicalDrive0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle hddHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                  FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (hddHandle.IsInvalid)
            {
                Console.WriteLine(@"!! Invalid {0}", drive);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper hddDeviceIo = new DeviceIOControlWrapper(hddHandle);

            DISK_GEOMETRY_EX info = hddDeviceIo.DiskGetDriveGeometryEx();

            Console.WriteLine("Sector size: " + info.Geometry.BytesPerSector);

            PARTITION_INFORMATION_EX partitionInfo = hddDeviceIo.DiskGetPartitionInfoEx();

            Console.WriteLine("Partition style: " + partitionInfo.PartitionStyle);

            Console.WriteLine();
        }

        private static void ExampleFileSystemIO()
        {
            const string drive = @"\\.\C:";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle volumeHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                Console.WriteLine(@"!! Invalid {0}", drive);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper volumeDeviceIo = new DeviceIOControlWrapper(volumeHandle);

            // Extract a complete file list from the target drive
            IUSN_RECORD[] usnData = volumeDeviceIo.FileSystemEnumUsnData();
            Console.WriteLine("Found {0:N0} file/folder records", usnData.Length);

            // Count the unique file names
            int usnNameUniques = new HashSet<string>(usnData.Select(s => s.FileName)).Count;
            Console.WriteLine("Found {0:N0} unique names on records", usnNameUniques);

            // Prepare a dictionary to resolve parents
            Dictionary<ulong, USN_RECORD_V2> usnDic = usnData.OfType<USN_RECORD_V2>().ToDictionary(s => s.FileReferenceNumber);

            const string root = drive + "\\";

            List<string> files = new List<string>();
            List<string> parents = new List<string>();

            foreach (USN_RECORD_V2 usnRecord in usnData.OfType<USN_RECORD_V2>())
            {
                parents.Clear();

                USN_RECORD_V2 current = usnRecord;
                while (usnDic.ContainsKey(current.ParentFileReferenceNumber))
                {
                    current = usnDic[current.ParentFileReferenceNumber];
                    parents.Add(current.FileName);
                }

                parents.Reverse();

                string path = Path.Combine(root, Path.Combine(parents.ToArray()), usnRecord.FileName);
                files.Add(path);
            }

            // Sort all files in lexicographical order
            files.Sort();

            // FS Stats
            FileSystemStats[] fsStats = volumeDeviceIo.FileSystemGetStatistics();

            for (int i = 0; i < fsStats.Length; i++)
            {
                switch (fsStats[i].Stats.FileSystemType)
                {
                    case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_NTFS:
                        NTFS_STATISTICS ntfsStats = (NTFS_STATISTICS)fsStats[i].FSStats;
                        Console.WriteLine("Processor {0}: (NTFS)  MFT Reads/Writes: {1,7:N0} / {2,7:N0}", i, ntfsStats.MftReads, ntfsStats.MftWrites);
                        break;
                    case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_FAT:
                        FAT_STATISTICS fatStats = (FAT_STATISTICS)fsStats[i].FSStats;
                        Console.WriteLine("Processor {0}: (FAT)   Noncached Disk Reads/Writes: {1,7:N0} / {2,7:N0}", i, fatStats.NonCachedDiskReads, fatStats.NonCachedDiskWrites);
                        break;
                    case FILESYSTEM_STATISTICS_TYPE.FILESYSTEM_STATISTICS_TYPE_EXFAT:
                        EXFAT_STATISTICS exfatStats = (EXFAT_STATISTICS)fsStats[i].FSStats;
                        Console.WriteLine("Processor {0}: (EXFAT) Noncached Disk Reads/Writes: {1,7:N0} / {2,7:N0}", i, exfatStats.NonCachedDiskReads, exfatStats.NonCachedDiskWrites);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Bitmap
            VOLUME_BITMAP_BUFFER bitmap = volumeDeviceIo.FileSystemGetVolumeBitmap(0);

            Console.WriteLine("Bitmap: {0:N0} clusters", bitmap.Buffer.Length);

            int trues = 0, falses = 0;
            for (int i = 0; i < bitmap.Buffer.Length; i++)
                if (bitmap.Buffer[i])
                    trues++;
                else
                    falses++;

            Console.WriteLine("Allocated clusters: {0:N0}", trues);
            Console.WriteLine("Unallocated clusters: {0:N0}", falses);

            // NTFS Base LCN (always 0)
            RETRIEVAL_POINTER_BASE basePointer = volumeDeviceIo.FileSystemGetRetrievalPointerBase();
            Console.WriteLine("Base LCN: {0:N0}", basePointer.FileAreaOffset);

            Console.WriteLine();
        }

        private static void ExampleCdRomIO()
        {
            const string drive = @"\\.\CdRom0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle cdTrayHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (cdTrayHandle.IsInvalid)
            {
                Console.WriteLine(@"!! Invalid {0}", drive);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper cdTrayDeviceIo = new DeviceIOControlWrapper(cdTrayHandle);

            // Open tray
            Console.WriteLine("Opening {0}", drive);
            cdTrayDeviceIo.StorageEjectMedia();
            Console.WriteLine("Opened {0}", drive);

            Console.WriteLine(" .. (waiting 2 seconds)");
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // Close tray
            Console.WriteLine("Closing {0}", drive);
            cdTrayDeviceIo.StorageLoadMedia();
            Console.WriteLine("Closed {0}", drive);

            Console.WriteLine();
        }

        private static void ExampleBitmap()
        {
            const string drive = @"\\.\C:";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle volumeHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                Console.WriteLine(@"!! Invalid {0}", drive);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper volumeDeviceIo = new DeviceIOControlWrapper(volumeHandle);

            // Bitmap
            VOLUME_BITMAP_BUFFER bitmap = volumeDeviceIo.FileSystemGetVolumeBitmap(0);

            Console.WriteLine("Bitmap: {0:N0} clusters", bitmap.Buffer.Length);

            int width = 10000;
            int height = (int)Math.Ceiling(bitmap.BitmapSize / (double)width);

            Console.WriteLine("W/H: {0:N0} / {1:N0}", width, height);

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                    g.Clear(Color.Green);

                for (int i = 0; i < bitmap.Buffer.Length; i++)
                {
                    int x = i % width;
                    int y = i / width;

                    if (bitmap.Buffer[i])
                        bmp.SetPixel(x, y, Color.Black);
                    else
                        bmp.SetPixel(x, y, Color.White);

                    if (y % 100 == 0 && x == 0)
                        Console.WriteLine("{0:N0} / {1:N0}", y, height);
                }

                bmp.Save("test.png", ImageFormat.Png);
            }

            Console.WriteLine();
        }
    }
}
