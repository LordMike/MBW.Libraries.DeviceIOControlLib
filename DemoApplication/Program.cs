using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceIOControlLib;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;

namespace DemoApplication
{
    public class Program
    {
        public const uint FILE_READ_ATTRIBUTES = (0x0080);
        public const uint FILE_WRITE_ATTRIBUTES = 0x0100;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
           string lpFileName,
           [MarshalAs(UnmanagedType.U4)] uint dwDesiredAccess,
           [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
           [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        private static void Main()
        {
            // Read disk sector size
            //ExampleDiskIO();

            //// Read volume info
            //ExampleFileSystemIO();

            // Defragment files
            //DefragLargeFiles();
            //DefragFreeSpace();
            //ExampleDefragmentFile(@"E:\Mike\Virtual Machines\Debian\Debian.vmdk");
            //ExampleDefragmentDir();
            ExampleDefragmentAll();

            //// Open and close CD Rom tray
            //ExampleCdRomIO();

            //// Draw a bitmap image of the disk allocation bitmap
            //ExampleBitmap();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void ExampleDiskIO()
        {
            const string drive = @"\\.\PhysicalDrive0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle hddHandle = CreateFile(drive, FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                  FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (hddHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
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
            SafeFileHandle volumeHandle = CreateFile(drive, FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
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

        private static void ExampleDefragmentFile(string file)
        {
            if (!File.Exists(file))
            {
                // Early exit
                Console.WriteLine("No such file: " + file);
                return;
            }

            char drive = Directory.GetDirectoryRoot(file)[0];

            Console.WriteLine(@"## Exmaple on {0} on drive {1} ##", file, drive);

            DefragmentEnvironment env = new DefragmentEnvironment(drive);

            DefragmentFile(env, env.FileNames[file]);
        }

        private static void ExampleDefragmentDir()
        {
            const string dir = @"E:\Mike\SVN";
            const char drive = 'E';

            Console.WriteLine(@"## Exmaple on drive {0} ##", drive);

            DefragmentEnvironment env = new DefragmentEnvironment('E');

            FileInfoes[] relevantFiles = env.FileNames.Where(s => s.Key.StartsWith(dir, StringComparison.InvariantCultureIgnoreCase)).Select(s => s.Value).ToArray();

            Console.WriteLine("Beginning work on {0} files", relevantFiles.Length);

            foreach (FileInfoes file in relevantFiles)
            {
                DefragmentFile(env, file);
            }

            Console.WriteLine();
        }

        private static void ExampleDefragmentAll()
        {
            const char drive = 'E';

            Console.WriteLine(@"## Exmaple on drive {0} ##", drive);

            DefragmentEnvironment env = new DefragmentEnvironment('E');

            FileInfoes[] relevantFiles = env.Files.Where(s => s.Extents.Length > 1).ToArray();

            Console.WriteLine("Beginning work on {0} files", relevantFiles.Length);

            foreach (FileInfoes file in relevantFiles)
            {
                DefragmentFile(env, file);
            }

            Console.WriteLine();
        }

        private static void DefragmentFile(DefragmentEnvironment env, FileInfoes file)
        {
            Console.WriteLine("Beginning work on {0}", file.FilePath);

            // Find all fragments of the file
            if (file.Extents.Length <= 1)
            {
                Console.WriteLine("File is already defragmented");
                return;
            }

            Console.WriteLine("File has {0} fragments", file.Extents.Length);

            // Calculate number of clusters we need to find
            int clustersNeeded = (int)file.Extents.Sum(s => (decimal)s.Size);

            Console.WriteLine("We need to find {0:N0} free clusters, for this file.", clustersNeeded);

            // Find a sequential area of [clustersNeeded] free clusters
            FreeBlock freeBlock = env.FindFreeBlock(0, (ulong)clustersNeeded);

            // Did we find a free area?
            if (freeBlock != null)
            {
                Console.WriteLine("Found {0:N0} free clusters starting at LCN: {1:N0}", freeBlock.Size, freeBlock.Lcn);

                for (int i = 0; i < file.Extents.Length; i++)
                {
                    ulong thisLcn = (ulong)(freeBlock.Lcn + file.Extents.Take(i).Sum(s => (decimal)s.Size));
                    if (!env.MoveFileData(file, file.Extents[i], thisLcn))
                    {
                        Console.WriteLine("Unable to move file: '{0}' extent #{1:N0} ({2:N0} clusters) to LCN {3:N0}", Path.GetFileName(file.FilePath), i, file.Extents[i].Size, thisLcn);
                        break;
                    }
                }

                env.CompactFileStructure(file);
            }
            else
            {
                Console.WriteLine("Unable to find {0:N0} contigous free clusters", clustersNeeded);
            }

            Console.WriteLine("File now has {0} fragment(s)", file.Extents.Length);
            Console.WriteLine();
        }

        private static void DefragFreeSpace()
        {
            const char drive = 'E';

            Console.WriteLine(@"## Exmaple on drive {0} ##", drive);

            // Open volume to defragment on
            DefragmentEnvironment env = new DefragmentEnvironment(drive);

            HashSet<ulong> ignoreFiles = new HashSet<ulong>();

            ulong diskPointer = 0;
            do
            {
                // Find freespace
                FreeBlock freeSpace = env.FindFreeBlock(diskPointer);

                if (freeSpace == null)
                {
                    Console.WriteLine("No freespace!");
                    break;
                }

                diskPointer = freeSpace.Lcn - 1;  // If we rerun with this number, we end at the same freespace

                Console.WriteLine("Handling freespace block at {0:N0} of {1:N0} clusters", freeSpace.Lcn, freeSpace.Size);

                // Clean the extents list - keep only files with fragments above our current freespace location
                List<FileInfoes> extentInfos = env.Files.Where(file => file.Extents.Any(extent => extent.Lcn > freeSpace.Lcn)).OrderByDescending(file => file.Extents.Max(extent => extent.Size)).ToList();

                if (!extentInfos.Any())
                {
                    Console.WriteLine("No extents left!");
                    break;
                }

                // Find the biggest possible fragment that can be moved to this freespace
                FileInfoes largestFile = extentInfos.FirstOrDefault(s => !ignoreFiles.Contains(s.FirstLCN) && s.Extents.Any(x => x.Size < freeSpace.Size));
                FileExtentInfo largestExtent;

                // Either create a bigger gap for the next run, or move something into the gap
                if (largestFile == null)
                {
                    // Select next fragment
                    largestExtent = env.ExtentLocations[freeSpace.Lcn + freeSpace.Size];
                    largestFile = extentInfos.First(s => s.Extents.Any(x => x.Lcn == largestExtent.Lcn));

                    // Find temporary location
                    FreeBlock tmpFreespace = env.FindFreeBlock(diskPointer, largestExtent.Size);

                    if (tmpFreespace == null)
                    {
                        Console.WriteLine("No possible fragment to move!");
                        diskPointer += freeSpace.Size;     // Advance to the next freespace
                        continue;
                    }

                    // Move file to temporary location
                    Console.WriteLine("Moving next fragment at {0:N0} of {1:N0} clusters to a temporary space at {2:N0}", largestExtent.Lcn, largestExtent.Size, tmpFreespace.Lcn);

                    bool success = env.MoveFileData(largestFile, largestExtent, tmpFreespace.Lcn);

                    if (!success)
                    {
                        Console.WriteLine("Unable to move temporary fragment!");
                        diskPointer += freeSpace.Size;     // Advance to the next freespace
                        continue;
                    }
                }
                else
                {
                    largestExtent = largestFile.Extents.Where(s => s.Size < freeSpace.Size).OrderByDescending(s => s.Size).First();

                    if (largestFile.Extents.Length == 1)
                        Console.WriteLine("Found a file at {0:N0} of {1:N0} clusters", largestExtent.Lcn, largestExtent.Size);
                    else
                        Console.WriteLine("Found a fragment at {0:N0} of {1:N0} clusters", largestExtent.Lcn, largestExtent.Size);

                    // Move this
                    bool success = env.MoveFileData(largestFile, largestExtent, freeSpace.Lcn);

                    if (!success)
                    {
                        Console.WriteLine();
                        Console.WriteLine(@"Could not place files");
                        ignoreFiles.Add(largestFile.FirstLCN);
                        Debugger.Break();
                        continue;
                    }

                    // Update pointer for next file
                    diskPointer += largestExtent.Size;

                    // We managed to move something, clear the ignore files
                    ignoreFiles.Clear();
                }

                Console.WriteLine();
            } while (true);

            Console.WriteLine();
        }

        private static void DefragLargeFiles()
        {
            const char drive = 'E';

            Console.WriteLine(@"## Exmaple on drive {0} ##", drive);

            // Open volume to defragment on
            DefragmentEnvironment env = new DefragmentEnvironment(drive);

            // Handle all fragmented files
            List<FileInfoes> fragmentedFiles = env.Files.Where(s => s.TotalSize < 1000 && s.Extents.Length > 1).ToList();

            for (int i = 0; i < fragmentedFiles.Count; i++)
            {
                FreeBlock freeBlock;
                FileInfoes file = fragmentedFiles[i];

                // Can we move the rest of the file up to after the first extent?
                FileExtentInfo firstExtent = file.Extents[0];
                if (!env.Bitmap.Buffer[(int)(firstExtent.Lcn + firstExtent.Size)])
                {
                    // There is a free space, let's see how big it is
                    freeBlock = env.FindFreeBlock(firstExtent.Lcn + firstExtent.Size);
                    decimal remainingSize = file.Extents.Skip(1).Sum(s => (decimal)s.Size);

                    if (freeBlock.Size >= remainingSize)
                    {
                        // It's big enough!
                        // Move file there
                        for (int j = 1; j < file.Extents.Length; j++)
                        {
                            ulong thisLcn = (ulong)(freeBlock.Lcn + file.Extents.Take(j - 1).Sum(s => (decimal)s.Size));
                            if (!env.MoveFileData(file, file.Extents[j], thisLcn))
                            {
                                // Something went wrong
                                break;
                            }
                        }

                        // Compact internal structures
                        env.CompactFileStructure(file);

                        // Process next fragmented file
                        continue;
                    }
                }

                // Can we move the entire file somewhere?
                freeBlock = env.FindFreeBlock(0, file.TotalSize);

                if (freeBlock != null)
                {
                    // Move file there
                    for (int j = 0; j < file.Extents.Length; j++)
                    {
                        env.MoveFileData(file, file.Extents[j], (ulong)(freeBlock.Lcn + file.Extents.Take(j).Sum(s => (decimal)s.Size)));
                    }

                    // Compact internal structures
                    env.CompactFileStructure(file);

                    // Process next fragmented file
                    continue;
                }

                //// Find largest fragment
                //var fragment = file.Extents.Select((extent, index) => new {extent, index}).MaxItem(s => s.extent.Size);


            }

            Console.WriteLine();
        }

        private static void ExampleCdRomIO()
        {
            const string drive = @"\\.\CdRom0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle cdTrayHandle = CreateFile(drive, FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (cdTrayHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
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
            SafeFileHandle volumeHandle = CreateFile(drive, FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (volumeHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
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

    public class DefragmentEnvironment
    {
        public VOLUME_BITMAP_BUFFER Bitmap { get; set; }
        public Dictionary<ulong, FileExtentInfo> ExtentLocations { get; set; }
        public List<FileInfoes> Files { get; set; }
        public Dictionary<string, FileInfoes> FileNames { get; set; }

        private DeviceIOControlWrapper _volumeDeviceIo;
        private string _driveRoot;

        public DefragmentEnvironment(char driveLetter)
        {
            PrepareVolume(driveLetter);

            Task tBitmap = Task.Factory.StartNew(UpdateBitmap).ContinueWith(task => Debug.WriteLine("Fetched bitmap, {0:N0} clusters", Bitmap.BitmapSize));
            Task tFiles = Task.Factory.StartNew(UpdateFiles).ContinueWith(task => Debug.WriteLine("Fetched {0:N0} files", Files.Count));

            tBitmap.Wait();
            tFiles.Wait();
        }

        private void PrepareVolume(char driveLetter)
        {
            string drive = @"\\.\" + driveLetter + ":";

            SafeFileHandle volumeHandle = Program.CreateFile(drive, Program.FILE_READ_ATTRIBUTES | Program.FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            _volumeDeviceIo = new DeviceIOControlWrapper(volumeHandle);
            _driveRoot = driveLetter + ":\\";
        }

        private void UpdateBitmap()
        {
            // Get bitmap
            Bitmap = _volumeDeviceIo.FileSystemGetVolumeBitmap();
        }

        private void UpdateFiles()
        {
            FileNames = new Dictionary<string, FileInfoes>();

            // Find all files
            List<USN_RECORD_V2> usns = _volumeDeviceIo.FileSystemEnumUsnData().OfType<USN_RECORD_V2>().ToList();
            Dictionary<ulong, USN_RECORD_V2> usnDic = usns.ToDictionary(s => s.FileReferenceNumber);

            List<string> files = new List<string>();
            List<string> parents = new List<string>();

            foreach (USN_RECORD_V2 usnRecord in usns)
            {
                if (usnRecord.FileAttributes.HasFlag(DeviceIOControlLib.FileAttributes.Directory))
                    continue;

                parents.Clear();

                USN_RECORD_V2 current = usnRecord;
                while (usnDic.ContainsKey(current.ParentFileReferenceNumber))
                {
                    current = usnDic[current.ParentFileReferenceNumber];
                    parents.Add(current.FileName);
                }

                parents.Reverse();

                string path = Path.Combine(_driveRoot, Path.Combine(parents.ToArray()), usnRecord.FileName);
                files.Add(path);
            }

            Files = new List<FileInfoes>();

            foreach (string file in files)
            {
                FileExtentInfo[] extents;
                using (SafeFileHandle fileHandle = Program.CreateFile(file, Program.FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                                      FileMode.Open, FileAttributes.Normal, IntPtr.Zero))
                {
                    if (fileHandle.IsInvalid)
                    {
                        int lastError = Marshal.GetLastWin32Error();

                        Debug.WriteLine(@"Could not open {0}: {1}", file, new Win32Exception(lastError).Message);
                        continue;
                    }

                    DeviceIOControlWrapper fileDeviceIo = new DeviceIOControlWrapper(fileHandle);

                    try
                    {
                        extents = fileDeviceIo.FileSystemGetRetrievalPointers();
                    }
                    catch (Win32Exception ex)
                    {
                        Debug.WriteLine(@"Could not get fragments {0}: {1}", file, ex.Message);
                        continue;
                    }
                }

                if (extents.Length > 0)
                {
                    FileInfoes fileInfoes = new FileInfoes(file, extents);
                    Files.Add(fileInfoes);
                    FileNames[file] = fileInfoes;
                }
            }

            ExtentLocations = Files.SelectMany(s => s.Extents).ToDictionary(s => s.Lcn);
        }

        public bool MoveFileData(FileInfoes file, FileExtentInfo extent, ulong newLcn)
        {
            Debug.Assert(file.Extents.Contains(extent));

            try
            {
                using (SafeFileHandle fileHandle = Program.CreateFile(file.FilePath, Program.FILE_READ_ATTRIBUTES,
                                                           FileShare.ReadWrite, IntPtr.Zero, FileMode.Open,
                                                           FileAttributes.Normal, IntPtr.Zero))
                {
                    _volumeDeviceIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), extent.Vcn, newLcn, (uint)extent.Size);
                }
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine("Unable to move extent VCN: {0:N0} (size: {1:N0}, LCN: {2:N0}) of file '{3}' to {4:N0}: {5}", extent.Vcn, extent.Size, extent.Lcn, file.FilePath, newLcn, ex.Message);
                return false;
            }

            // Update internal structures
            // Update bitmap, mark extent as freed
            for (ulong i = extent.Lcn; i < extent.Lcn + extent.Size; i++)
            {
                Bitmap.Buffer[(int)i] = false;
            }

            // Update bitmap, mark new area as used
            for (ulong i = newLcn; i < newLcn + extent.Size; i++)
            {
                // Mark as used
                Bitmap.Buffer[(int)i] = true;
            }

            // Update file extent
            ExtentLocations.Remove(extent.Lcn);
            ExtentLocations.Add(newLcn, extent);

            extent.Lcn = newLcn;
            file.UpdateMeta();

            return true;
        }

        public void CompactFileStructure(FileInfoes file)
        {
            if (file.Extents.Length <= 1)
                return;

            for (int i = 1; i < file.Extents.Length; i++)
            {
                // Join this one with the previous one
                if (JoinExtents(file, file.Extents[i - 1], file.Extents[i]))
                    i--;
            }

            // Update file structure
            file.UpdateMeta();
        }

        private Tuple<FileExtentInfo, FileExtentInfo> SplitExtent(FileInfoes file, FileExtentInfo extent, ulong firstItemSize)
        {
            Debug.Assert(firstItemSize < extent.Size);
            Debug.Assert(file.Extents.Contains(extent));

            // Create new extent
            FileExtentInfo secondExtent = new FileExtentInfo();
            secondExtent.Vcn = extent.Vcn + firstItemSize;
            secondExtent.Lcn = extent.Lcn + firstItemSize;
            secondExtent.Size = extent.Size - firstItemSize;

            // Shrink existing extent
            extent.Lcn -= firstItemSize;
            extent.Vcn -= firstItemSize;
            extent.Size = firstItemSize;

            // Update file structure
            file.Extents = file.Extents.Concat(new[] { secondExtent }).OrderBy(s => s.Vcn).ToArray();

            // Update internal structures
            // Add secondExtent
            ExtentLocations.Add(secondExtent.Lcn, secondExtent);

            return new Tuple<FileExtentInfo, FileExtentInfo>(extent, secondExtent);
        }

        private bool JoinExtents(FileInfoes file, FileExtentInfo a, FileExtentInfo b)
        {
            Debug.Assert(file.Extents.Contains(a));
            Debug.Assert(file.Extents.Contains(b));

            if (a.Lcn > b.Lcn)
            {
                // Swap
                var tmp = a;
                a = b;
                b = tmp;
            }

            if (a.Vcn + a.Size == b.Vcn && a.Lcn + a.Size == b.Lcn)
            {
                // The two extents are neighbours, both logically and physically on disk
                // Increase a
                a.Size += b.Size;

                // Remove b from the list
                file.Extents = file.Extents.Where(s => s.Lcn != b.Lcn).OrderBy(s => s.Vcn).ToArray();

                // Update internal structures
                // Remove b
                ExtentLocations.Remove(b.Lcn);

                return true;
            }

            return false;
        }

        public FreeBlock FindFreeBlock(ulong startLcn = 0, ulong minSize = 1)
        {
            startLcn = startLcn == 0 ? 1 : startLcn;

            ulong lastFreeStart = 0;
            for (ulong i = startLcn; i < Bitmap.BitmapSize; i++)
            {
                if (Bitmap.Buffer[(int)(i - 1)] && !Bitmap.Buffer[(int)i])
                {
                    // Switching from used to free
                    lastFreeStart = i;
                }
                else if (!Bitmap.Buffer[(int)(i - 1)] && Bitmap.Buffer[(int)i] && lastFreeStart >= startLcn)
                {
                    // Switching from free to used
                    if (i - lastFreeStart >= minSize)
                        return new FreeBlock(lastFreeStart, i - lastFreeStart);
                }
            }

            return null;
        }
    }

    public class FreeBlock
    {
        public ulong Lcn { get; set; }
        public ulong Size { get; set; }

        public FreeBlock(ulong lcn, ulong size)
        {
            Lcn = lcn;
            Size = size;
        }
    }

    public class FileInfoes
    {
        public FileExtentInfo[] Extents { get; set; }
        public ulong FirstLCN { get; set; }
        public uint TotalSize { get; set; }
        public string FilePath { get; set; }

        public FileInfoes(string filePath, FileExtentInfo[] extents)
        {
            FilePath = filePath;

            Extents = extents;
            UpdateMeta();
        }

        public void UpdateMeta()
        {
            FirstLCN = Extents.Any() ? Extents.Min(s => s.Lcn) : 0;
            TotalSize = Extents.Any() ? (uint)Extents.Sum(s => (uint)s.Size) : 0;
        }
    }

    public static class EnumerableExtensions
    {
        public static T MaxItem<T, V>(this IEnumerable<T> items, Func<T, V> selector) where V : IComparable<V>
        {
            T max = default(T);

            if (!items.Any())
                return max;

            // Find max item
            max = items.First();
            V maxVal = selector(max);

            foreach (T item in items.Skip(1))
            {
                V currentVal = selector(item);
                if (currentVal.CompareTo(maxVal) <= 0)       // currentVal <= maxVal
                    continue;

                maxVal = currentVal;
                max = item;
            }

            return max;
        }
    }
}
