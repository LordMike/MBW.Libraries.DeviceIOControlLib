using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.FileSystem;
using DeviceIOControlLib.Objects.MountManager;
using DeviceIOControlLib.Objects.Storage;
using DeviceIOControlLib.Objects.Usn;
using DeviceIOControlLib.Wrapper;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;

namespace DemoApplication
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
           string lpFileName,
           [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
           [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
           [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        private static extern uint GetCompressedFileSize(string lpFileName, out uint lpFileSizeHigh);

        private static long GetCompressedSize(string fileName)
        {
            uint highOrder;
            long lowOrder = GetCompressedFileSize(fileName, out highOrder);
            long tmp = (long)highOrder << 32;

            return tmp | lowOrder;
        }

        private static void Main()
        {
            // Read mount points
            ExampleMountManager();

            // Read physical drive
            ExamplePhysicalDrive();

            // Read USN Journal
            ExampleUsnJournal();

            // Read disk sector size
            ExampleDiskIO();

            // Read volume info
            ExampleFileSystemIO();

            // Work with sparse files
            ExampleSparseFile();

            // Work with compressed files
            ExampleCompression();

            // Defragment files
            //ExampleDefragmentFile();
            //ExampleDefragmentDir();

            // Open and close CD Rom tray
            ExampleCdRomIO();

            // Draw a bitmap image of the disk allocation bitmap
            ExampleBitmap();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void ExampleUsnJournal()
        {
            const string drive = @"\\.\C:";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle hddHandle = CreateFile(drive, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (hddHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (UsnDeviceWrapper usnIo = new UsnDeviceWrapper(hddHandle, true))
            {
                USN_JOURNAL_DATA_V0 data = usnIo.FileSystemQueryUsnJournal();

                Console.WriteLine("UsnJournalID: {0:N0}", data.UsnJournalID);
                Console.WriteLine("FirstUsn #: {0:N0}", data.FirstUsn.Usn);
                Console.WriteLine("NextUsn #: {0:N0}", data.NextUsn.Usn);
                Console.WriteLine("LowestValidUsn #: {0:N0}", data.LowestValidUsn.Usn);
                Console.WriteLine("MaxUsn #: {0:N0}", data.MaxUsn.Usn);
                Console.WriteLine("MaximumSize: {0:N0}", data.MaximumSize);
                Console.WriteLine("AllocationDelta: {0:N0}", data.AllocationDelta);
            }

            Console.WriteLine();
        }

        private static void ExampleMountManager()
        {
            const string device = @"\\.\MountPointManager";

            Console.WriteLine(@"## Exmaple on {0} ##", device);
            SafeFileHandle deviceHandle = CreateFile(device, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (deviceHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", device, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (MountManagerWrapper mountManager = new MountManagerWrapper(deviceHandle, true))
            {
                List<MountPoint> mountPoints = mountManager.MountQueryPoints();

                foreach (MountPoint mountPoint in mountPoints)
                {
                    Console.WriteLine("Mount point:");
                    Console.WriteLine($"- Device Name: {mountPoint.DeviceName}");
                    Console.WriteLine($"- Symbolic link name: {mountPoint.SymbolicLinkName}");
                    Console.WriteLine($"- Device ID: {BitConverter.ToString(mountPoint.UniqueId).Replace("-", "")}");

                    List<string> volumePaths = mountManager.QueryDosVolumePaths(mountPoint.DeviceName);

                    Console.WriteLine($"- Paths ({volumePaths.Count:N0}): ");
                    foreach (string volumePath in volumePaths)
                        Console.WriteLine($"- Path: {volumePath}");

                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        private static void ExampleDiskIO()
        {
            const string drive = @"\\.\PhysicalDrive0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle hddHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (hddHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (DiskDeviceWrapper diskIo = new DiskDeviceWrapper(hddHandle, true))
            {
                DISK_GEOMETRY_EX info = diskIo.DiskGetDriveGeometryEx();

                Console.WriteLine("Sector size: " + info.Geometry.BytesPerSector);

                switch (info.PartitionInformation.PartitionStyle)
                {
                    case PartitionStyle.PARTITION_STYLE_MBR:
                        Console.WriteLine("MBR Id: " + info.PartitionInformation.MbrSignature);
                        break;
                    case PartitionStyle.PARTITION_STYLE_GPT:
                        Console.WriteLine("GPT GUID: " + info.PartitionInformation.GptGuidId);
                        break;
                }

                PARTITION_INFORMATION_EX partitionInfo = diskIo.DiskGetPartitionInfoEx();

                Console.WriteLine("Partition style: " + partitionInfo.PartitionStyle);
            }

            Console.WriteLine();
        }

        private static void ExampleFileSystemIO()
        {
            const string drive = @"\\.\C:";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle volumeHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (volumeHandle)
            {
                // Extract a complete file list from the target drive
                IUSN_RECORD[] usnData;
                using (UsnDeviceWrapper usnIo = new UsnDeviceWrapper(volumeHandle))
                    usnData = usnIo.FileSystemEnumUsnData();

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
                FileSystemStats[] fsStats;
                using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(volumeHandle))
                    fsStats = fsIo.FileSystemGetStatistics();

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
                VOLUME_BITMAP_BUFFER bitmap;
                using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(volumeHandle))
                    bitmap = fsIo.FileSystemGetVolumeBitmap(0);

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
                RETRIEVAL_POINTER_BASE basePointer;
                using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(volumeHandle))
                    basePointer = fsIo.FileSystemGetRetrievalPointerBase();
                Console.WriteLine("Base LCN: {0:N0}", basePointer.FileAreaOffset);
            }

            Console.WriteLine();
        }

        private static void ExampleDefragmentFile()
        {
            const string file = @"C:\Windows\system32\cmd.exe";
            string drive = @"\\.\" + Directory.GetDirectoryRoot(file).Substring(0, 2);

            Console.WriteLine(@"## Exmaple on {0} on drive {1} ##", file, drive);

            // Open file to defragment
            SafeFileHandle fileHandle = CreateFile(file, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (fileHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            // Open volume to defragment on
            SafeFileHandle driveHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (driveHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (FilesystemDeviceWrapper fileIo = new FilesystemDeviceWrapper(fileHandle, true))
            using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(driveHandle, true))
            {
                // Find all fragments of the file
                FileExtentInfo[] extents = fileIo.FileSystemGetRetrievalPointers();
                Console.WriteLine("File has {0} fragments", extents.Length);

                if (extents.Length == 1)
                {
                    Console.WriteLine("File is already defragmented");
                    Console.WriteLine();
                    return;
                }

                // Calculate number of clusters we need to find
                int clustersNeeded = (int)extents.Sum(s => (decimal)s.Size);

                Console.WriteLine("We need to find {0:N0} free clusters, for this file.", clustersNeeded);

                // Get the drive bitmap
                VOLUME_BITMAP_BUFFER bitmap = fsIo.FileSystemGetVolumeBitmap();

                Console.WriteLine("Got the drive bitmap, it contains {0:N0} clusters and starts at LCN: {1:N0}", bitmap.BitmapSize, bitmap.StartingLcn);

                // Find a sequential area of [clustersNeeded] free clusters
                ulong foundFreeLCN = 0;      // The result of the search
                uint maxStart = (uint)(bitmap.Buffer.Length - (decimal)clustersNeeded); // There's no point searching beyond this LCN

                // Enumerate clusters
                for (uint i = 0; i < maxStart; i++)
                {
                    // Check bitmap, find location with [clustersNeeded] free clusters
                    bool found = true;
                    for (uint x = i; x < i + clustersNeeded; x++)
                    {
                        if (bitmap.Buffer[(int)x])
                        {
                            // Found an occupied cluster
                            found = false;

                            // Advance the pointer, so that we don't have to re-search the same clusters again.
                            i = x;

                            break;
                        }
                    }

                    if (found)
                    {
                        // We found a free block!
                        foundFreeLCN = i + bitmap.StartingLcn;
                        break;
                    }
                }

                // Did we find a free area?
                if (foundFreeLCN > 0)
                {
                    Console.WriteLine("Found {0:N0} free clusters starting at LCN: {1:N0}", clustersNeeded, foundFreeLCN);

                    fsIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), 0, foundFreeLCN, (uint)clustersNeeded);
                }
                else
                {
                    Console.WriteLine("Unable to find {0:N0} contigous free clusters", clustersNeeded);
                }

                extents = fileIo.FileSystemGetRetrievalPointers();
                Console.WriteLine("File now has {0} fragment(s)", extents.Length);
            }

            Console.ReadLine();
            Console.WriteLine();
        }

        private static void ExampleDefragmentDir()
        {
            const string dir = @"C:\Windows";
            string drive = @"\\.\" + Directory.GetDirectoryRoot(dir).Substring(0, 2);

            Console.WriteLine(@"## Exmaple on {0} on drive {1} ##", dir, drive);

            // Open volume to defragment on
            SafeFileHandle driveHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (driveHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(driveHandle, true))
            {
                // Get the drive bitmap
                VOLUME_BITMAP_BUFFER bitmap = fsIo.FileSystemGetVolumeBitmap();

                Console.WriteLine("Got the drive bitmap, it contains {0:N0} clusters and starts at LCN: {1:N0}", bitmap.BitmapSize, bitmap.StartingLcn);

                string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
                Console.WriteLine("Beginning work on {0} files", files.Length);

                foreach (string file in files)
                {
                    Console.WriteLine("Beginning work on {0}", file);

                    // Open file to defragment
                    SafeFileHandle fileHandle = CreateFile(file, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                                                             FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                    if (fileHandle.IsInvalid)
                    {
                        int lastError = Marshal.GetLastWin32Error();

                        Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                        continue;
                    }

                    using (FilesystemDeviceWrapper fileIo = new FilesystemDeviceWrapper(fileHandle, true))
                    {
                        // Find all fragments of the file
                        FileExtentInfo[] extents;
                        try
                        {
                            extents = fileIo.FileSystemGetRetrievalPointers();
                        }
                        catch (Win32Exception ex)
                        {
                            Console.WriteLine("Couldn't get file extents: " + ex.Message);
                            continue;
                        }

                        if (extents.Length == 1)
                        {
                            Console.WriteLine("File is already defragmented");
                            continue;
                        }
                        Console.WriteLine("File has {0} fragments", extents.Length);

                        // Calculate number of clusters we need to find
                        int clustersNeeded = (int)extents.Sum(s => (decimal)s.Size);

                        Console.WriteLine("We need to find {0:N0} free clusters, for this file.", clustersNeeded);

                        // Find a sequential area of [clustersNeeded] free clusters
                        ulong foundFreeLCN = 0;      // The result of the search
                        uint maxStart = (uint)(bitmap.Buffer.Length - (decimal)clustersNeeded); // There's no point searching beyond this LCN

                        // Enumerate clusters
                        for (uint i = 0; i < maxStart; i++)
                        {
                            // Check bitmap, find location with [clustersNeeded] free clusters
                            bool found = true;
                            for (uint x = i; x < i + clustersNeeded; x++)
                            {
                                if (bitmap.Buffer[(int)x])
                                {
                                    // Found an occupied cluster
                                    found = false;

                                    // Advance the pointer, so that we don't have to re-search the same clusters again.
                                    i = x;

                                    break;
                                }
                            }

                            if (found)
                            {
                                // We found a free block!
                                foundFreeLCN = i + bitmap.StartingLcn;
                                break;
                            }
                        }

                        // Did we find a free area?
                        if (foundFreeLCN > 0)
                        {
                            Console.WriteLine("Found {0:N0} free clusters starting at LCN: {1:N0}", clustersNeeded, foundFreeLCN);

                            try
                            {
                                fsIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), 0, foundFreeLCN, (uint)clustersNeeded);

                                // Mark newly used areas as used
                                ulong upperFreeLCN = foundFreeLCN + (ulong)clustersNeeded;
                                for (ulong i = foundFreeLCN; i < upperFreeLCN; i++)
                                {
                                    bitmap.Buffer[(int)i] = true;
                                }

                                // Mark newly freed areas as free
                                foreach (FileExtentInfo extent in extents)
                                {
                                    for (ulong i = extent.Lcn; i < extent.Lcn + extent.Size; i++)
                                    {
                                        bitmap.Buffer[(int)i] = false;
                                    }
                                }
                            }
                            catch (Win32Exception ex)
                            {
                                Console.WriteLine("Unable to move file: " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unable to find {0:N0} contigous free clusters", clustersNeeded);
                        }

                        extents = fileIo.FileSystemGetRetrievalPointers();
                        Console.WriteLine("File now has {0} fragment(s)", extents.Length);
                    }
                }
            }

            Console.WriteLine("Done... ");
            Console.ReadLine();
            Console.WriteLine();
        }

        private static void ExamplePhysicalDrive()
        {
            const string drive = @"\\.\PhysicalDrive0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle diskHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (diskHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (StorageDeviceWrapper storageIo = new StorageDeviceWrapper(diskHandle, true))
            {
                // Get info
                STORAGE_DEVICE_DESCRIPTOR_PARSED prop = storageIo.StorageGetDeviceProperty();

                Console.WriteLine(prop.BusType);

                Console.WriteLine("Version: " + prop.Version);
                Console.WriteLine("Size: " + prop.Size);
                Console.WriteLine("DeviceType: " + prop.DeviceType);
                Console.WriteLine("DeviceTypeModifier: " + prop.DeviceTypeModifier);
                Console.WriteLine("RemovableMedia: " + prop.RemovableMedia);
                Console.WriteLine("CommandQueueing: " + prop.CommandQueueing);
                Console.WriteLine("VendorIdOffset: " + prop.VendorIdOffset);
                Console.WriteLine("ProductIdOffset: " + prop.ProductIdOffset);
                Console.WriteLine("ProductRevisionOffset: " + prop.ProductRevisionOffset);
                Console.WriteLine("SerialNumberOffset: " + prop.SerialNumberOffset);
                Console.WriteLine("BusType: " + prop.BusType);
                Console.WriteLine("RawPropertiesLength: " + prop.RawPropertiesLength);
                Console.WriteLine("SerialNumber: " + prop.SerialNumber?.Trim());
                Console.WriteLine("ProductId: " + prop.ProductId?.Trim());
                Console.WriteLine("VendorId: " + prop.VendorId?.Trim());
                Console.WriteLine("ProductRevision: " + prop.ProductRevision?.Trim());
            }

            Console.WriteLine();
        }

        private static void ExampleCdRomIO()
        {
            const string drive = @"\\.\CdRom0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle cdTrayHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (cdTrayHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (StorageDeviceWrapper storageIo = new StorageDeviceWrapper(cdTrayHandle, true))
            {
                // Open tray
                Console.WriteLine("Opening {0}", drive);
                storageIo.StorageEjectMedia();
                Console.WriteLine("Opened {0}", drive);

                Console.WriteLine(" .. (waiting 2 seconds)");
                Thread.Sleep(TimeSpan.FromSeconds(2));

                // Close tray
                Console.WriteLine("Closing {0}", drive);
                storageIo.StorageLoadMedia();
                Console.WriteLine("Closed {0}", drive);
            }

            Console.WriteLine();
        }

        private static void ExampleBitmap()
        {
            const string drive = @"\\.\C:";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle volumeHandle = CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (FilesystemDeviceWrapper fsIo = new FilesystemDeviceWrapper(volumeHandle, true))
            {
                // Bitmap
                VOLUME_BITMAP_BUFFER bitmap = fsIo.FileSystemGetVolumeBitmap();

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
            }

            Console.WriteLine();
        }

        private static void ExampleSparseFile()
        {
            string file = Path.GetTempFileName();

            try
            {
                // Write file data
                const int length = 1 * 1024 * 1024; // 1 MB
                const int sparseLength = 512 * 1024; // 0.5 MB
                using (FileStream fs = File.OpenWrite(file))
                {
                    byte[] data = new byte[length];
                    for (int i = 0; i < data.Length; i++)
                        data[i] = 1;        // Set the data to non-zero
                    fs.Write(data, 0, data.Length);
                }

                Console.WriteLine(@"## Exmaple with {0} ##", file);
                SafeFileHandle fileHandle = CreateFile(file, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (fileHandle.IsInvalid)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                    Console.WriteLine();
                    return;
                }

                using (FilesystemDeviceWrapper fileIo = new FilesystemDeviceWrapper(fileHandle, true))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    Console.WriteLine("File size: " + fileInfo.Length);
                    Console.WriteLine("Sparse: " + fileInfo.Attributes.HasFlag(FileAttributes.SparseFile));

                    // Enable sparse ranges
                    Console.WriteLine("Enabling Sparse file");
                    fileIo.FileSystemSetSparseFile(true);

                    fileInfo.Refresh();
                    Console.WriteLine("Sparse: " + fileInfo.Attributes.HasFlag(FileAttributes.SparseFile));

                    // Set sparse range
                    Console.WriteLine("  Setting sparse range: " + sparseLength + " (length: " + ((fileInfo.Length - sparseLength) / 2) + ")");
                    fileIo.FileSystemSetZeroData(sparseLength, (fileInfo.Length - sparseLength) / 2);

                    // Query sparse range
                    fileInfo.Refresh();
                    FILE_ALLOCATED_RANGE_BUFFER[] ranges = fileIo.FileSystemQueryAllocatedRanges(0, length);
                    Console.WriteLine("File size: " + fileInfo.Length);

                    Console.WriteLine("Sparse ranges (" + ranges.Length + "):");
                    foreach (FILE_ALLOCATED_RANGE_BUFFER range in ranges)
                    {
                        Console.WriteLine("  Non-sparse range: " + range.FileOffset + " (length: " + range.Length + ")");
                    }
                }
            }
            finally
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        private static void ExampleCompression()
        {
            string file = Path.GetTempFileName();

            try
            {
                // Write file data
                using (FileStream fs = File.OpenWrite(file))
                {
                    byte[] data = Encoding.ASCII.GetBytes("The white bunny jumps over the brown dog in a carparking lot");

                    for (int i = 0; i < 20000; i++)
                    {
                        fs.Write(data, 0, data.Length);
                    }
                }

                Console.WriteLine(@"## Exmaple with {0} ##", file);
                SafeFileHandle fileHandle = CreateFile(file, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (fileHandle.IsInvalid)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                    Console.WriteLine();
                    return;
                }

                using (FilesystemDeviceWrapper fileIo = new FilesystemDeviceWrapper(fileHandle, true))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    Console.WriteLine("File size: {0:N0} (compressed: {1:N0})", fileInfo.Length, GetCompressedSize(file));
                    Console.WriteLine("Compression: {0} ({1})", fileInfo.Attributes.HasFlag(FileAttributes.Compressed), fileIo.FileSystemGetCompression());

                    Console.WriteLine("  Enabling compression (LZNT1)");
                    fileIo.FileSystemSetCompression(COMPRESSION_FORMAT.LZNT1);

                    fileInfo.Refresh();
                    Console.WriteLine("File size: {0:N0} (compressed: {1:N0})", fileInfo.Length, GetCompressedSize(file));
                    Console.WriteLine("Compression: {0} ({1})", fileInfo.Attributes.HasFlag(FileAttributes.Compressed), fileIo.FileSystemGetCompression());
                }
            }
            finally
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
    }
}
