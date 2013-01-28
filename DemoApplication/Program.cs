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
    class Program
    {
        const uint FILE_READ_ATTRIBUTES = (0x0080);
        const uint FILE_WRITE_ATTRIBUTES = 0x0100;

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
            //DefragFreeSpace();
            //ExampleDefragmentFile(@"E:\Mike\Virtual Machines\Debian\Debian.vmdk");
            ExampleDefragmentDir();

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
            string drive = @"\\.\" + Directory.GetDirectoryRoot(file).Substring(0, 2);

            Console.WriteLine(@"## Exmaple on {0} on drive {1} ##", file, drive);

            // Open file to defragment
            SafeFileHandle fileHandle = CreateFile(file, FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (fileHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper fileDeviceIo = new DeviceIOControlWrapper(fileHandle);

            // Open volume to defragment on
            SafeFileHandle driveHandle = CreateFile(drive, FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (driveHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper driveDeviceIo = new DeviceIOControlWrapper(driveHandle);

            // Find all fragments of the file
            FileExtentInfo[] extents = fileDeviceIo.FileSystemGetRetrievalPointers();
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
            VOLUME_BITMAP_BUFFER bitmap = driveDeviceIo.FileSystemGetVolumeBitmap();

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

                driveDeviceIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), 0, foundFreeLCN, (uint)clustersNeeded);
            }
            else
            {
                Console.WriteLine("Unable to find {0:N0} contigous free clusters", clustersNeeded);
            }

            extents = fileDeviceIo.FileSystemGetRetrievalPointers();
            Console.WriteLine("File now has {0} fragment(s)", extents.Length);

            Console.ReadLine();
            Console.WriteLine();
        }

        private static void ExampleDefragmentDir()
        {
            const string dir = @"E:\Mike\Dropbox\";
            string drive = @"\\.\" + Directory.GetDirectoryRoot(dir).Substring(0, 2);

            Console.WriteLine(@"## Exmaple on {0} on drive {1} ##", dir, drive);

            // Open volume to defragment on
            SafeFileHandle driveHandle = CreateFile(drive, FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (driveHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper driveDeviceIo = new DeviceIOControlWrapper(driveHandle);

            // Get the drive bitmap
            VOLUME_BITMAP_BUFFER bitmap = driveDeviceIo.FileSystemGetVolumeBitmap();

            Console.WriteLine("Got the drive bitmap, it contains {0:N0} clusters and starts at LCN: {1:N0}", bitmap.BitmapSize, bitmap.StartingLcn);

            string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            Console.WriteLine("Beginning work on {0} files", files.Length);

            foreach (string file in files)
            {
                Console.WriteLine("Beginning work on {0}", file);

                // Open file to defragment
                SafeFileHandle fileHandle = CreateFile(file, FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                         FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (fileHandle.IsInvalid)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", file, lastError, new Win32Exception(lastError).Message);
                    continue;
                }

                DeviceIOControlWrapper fileDeviceIo = new DeviceIOControlWrapper(fileHandle);

                // Find all fragments of the file
                FileExtentInfo[] extents;
                try
                {
                    extents = fileDeviceIo.FileSystemGetRetrievalPointers();
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine("Couldn't get file extents: " + ex.Message);
                    continue;
                }

                if (extents.Length <= 1)
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
                        driveDeviceIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), 0, foundFreeLCN, (uint)clustersNeeded);

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

                extents = fileDeviceIo.FileSystemGetRetrievalPointers();
                Console.WriteLine("File now has {0} fragment(s)", extents.Length);
            }

            Console.WriteLine("Done... ");
            Console.ReadLine();
            Console.WriteLine();
        }

        private static void DefragFreeSpace()
        {
            const string drive = @"\\.\E:";

            Console.WriteLine(@"## Exmaple on drive {0} ##", drive);

            // Open volume to defragment on
            SafeFileHandle driveHandle = CreateFile(drive, FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                     FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (driveHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            DeviceIOControlWrapper driveDeviceIo = new DeviceIOControlWrapper(driveHandle);

            //var data1 = driveDeviceIo.FileSystemGetNtfsFileRecord(2116);

            //string signature = Encoding.ASCII.GetString(data1.FileRecordBuffer, 0, 4);
            //var offsetToUpdateSeq = BitConverter.ToUInt16(data1.FileRecordBuffer, 4);
            //var updateSeqSizeWords = BitConverter.ToUInt16(data1.FileRecordBuffer, 6);
            //var logSeqNum = BitConverter.ToUInt64(data1.FileRecordBuffer, 8);
            //var seqNumReuse = BitConverter.ToUInt16(data1.FileRecordBuffer, 16);
            //var hardlinkCount = BitConverter.ToUInt16(data1.FileRecordBuffer, 18);
            //var offsetFirstAttrib = BitConverter.ToUInt16(data1.FileRecordBuffer, 20);
            //var flags = BitConverter.ToUInt16(data1.FileRecordBuffer, 22);
            //var realSizeMFTRec = BitConverter.ToUInt32(data1.FileRecordBuffer, 24);
            //var allocSizeMFTRec = BitConverter.ToUInt32(data1.FileRecordBuffer, 28);
            //var baseRec = BitConverter.ToUInt64(data1.FileRecordBuffer, 32);
            //var nextFreeAttrib = BitConverter.ToUInt16(data1.FileRecordBuffer, 40);
            //var recId = BitConverter.ToUInt32(data1.FileRecordBuffer, 44);
            //var usn = BitConverter.ToUInt16(data1.FileRecordBuffer, 48);
            //var usnArray = BitConverter.ToUInt16(data1.FileRecordBuffer, 50);

            //int attribPointer = offsetFirstAttrib;
            //do
            //{
            //    var attribType = BitConverter.ToUInt32(data1.FileRecordBuffer, attribPointer);
            //    if (attribType == uint.MaxValue)
            //        break;

            //    var attribLengthInclHeader = BitConverter.ToUInt32(data1.FileRecordBuffer, attribPointer + 4);
            //    var nonResidentFlag = data1.FileRecordBuffer[attribPointer + 8];
            //    var nameLength = data1.FileRecordBuffer[attribPointer + 9];
            //    var offsetToNameOrResidentData = BitConverter.ToUInt16(data1.FileRecordBuffer, attribPointer + 10);
            //    var attribFlags = BitConverter.ToUInt16(data1.FileRecordBuffer, attribPointer + 12);
            //    var attribId = BitConverter.ToUInt16(data1.FileRecordBuffer, attribPointer + 14);
            //    var attribLength = BitConverter.ToUInt32(data1.FileRecordBuffer, attribPointer + 16);
            //    var offsetToData = BitConverter.ToUInt16(data1.FileRecordBuffer, attribPointer + 20);
            //    var indexedFlag = data1.FileRecordBuffer[attribPointer + 22];

            //    var attribData = new byte[attribLength];
            //    Array.Copy(data1.FileRecordBuffer, attribPointer + offsetToData, attribData, 0, attribLength);

            //    attribPointer += (int)attribLengthInclHeader;
            //} while (true);

            // Find all files
            List<USN_RECORD_V2> usns = driveDeviceIo.FileSystemEnumUsnData().OfType<USN_RECORD_V2>().ToList();
            Dictionary<ulong, USN_RECORD_V2> usnDic = usns.ToDictionary(s => s.FileReferenceNumber);
            const string root = "E:\\";

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

                string path = Path.Combine(root, Path.Combine(parents.ToArray()), usnRecord.FileName);
                files.Add(path);
            }

            List<FileInfoes> extentInfos = new List<FileInfoes>();

            for (int index = 0; index < files.Count; index++)
            {
                string file = files[index];
                SafeFileHandle fileHandle = CreateFile(file, FILE_READ_ATTRIBUTES, FileShare.ReadWrite, IntPtr.Zero,
                                                       FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (fileHandle.IsInvalid)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    Console.WriteLine(@"Could not open {0}: {1}", file, new Win32Exception(lastError).Message);
                    continue;
                }

                DeviceIOControlWrapper fileDeviceIo = new DeviceIOControlWrapper(fileHandle);

                FileExtentInfo[] extents;
                try
                {
                    extents = fileDeviceIo.FileSystemGetRetrievalPointers();
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine(@"Could not get fragments {0}: {1}", file, ex.Message);
                    continue;
                }

                if (index % 1000 == 0)
                    Console.WriteLine("Opened file {0:N0} of {1:N0}, fragments: {2:N0}", index, files.Count, extents.Length);

                fileHandle.Close();

                if (extents.Length > 0)
                    extentInfos.Add(new FileInfoes(file, extents));
            }

            // Sort extents, last ones first
            extentInfos = extentInfos.OrderByDescending(s => s.FirstLCN).ToList();

            // Get bitmap
            VOLUME_BITMAP_BUFFER bitmap = driveDeviceIo.FileSystemGetVolumeBitmap();
            Console.WriteLine("Got Bitmap with {0:N0} clusters starting at LCN: {1:N0}", bitmap.BitmapSize, bitmap.StartingLcn);

            ulong diskPointer = 0;
            bool somethingDone;
            do
            {
                somethingDone = false;
                // Find freespace
                Tuple<ulong, ulong> freeSpace = FindFirstFreeSpace(bitmap, diskPointer);

                if (freeSpace.Item1 == 0)
                    Debugger.Break();

                // Find suitable partner
                Console.WriteLine("Finding best fit for {0:N0} free clusters at {1:N0}", freeSpace.Item2, freeSpace.Item1);

                int freeSpaceLeft = (int)freeSpace.Item2;
                List<FileInfoes> filesToMove = new List<FileInfoes>();
                HashSet<ulong> foundIndices = new HashSet<ulong>();

                extentInfos = extentInfos.Where(s => s.FirstLCN > freeSpace.Item1 + freeSpace.Item2).OrderByDescending(s => s.FirstLCN).ToList();

                Console.Write("Found {0:N0} candidates, {1:N0} clusters remaining", filesToMove.Count, freeSpaceLeft);
                do
                {
                    // Find best fit from the end of the drive
                    FileInfoes x = extentInfos.FirstOrDefault(s => s.TotalSize <= freeSpaceLeft && s.FirstLCN > freeSpace.Item1 + freeSpace.Item2 && !foundIndices.Contains(s.FirstLCN));

                    if (x == null)
                        break;

                    freeSpaceLeft -= (int)x.TotalSize;
                    filesToMove.Add(x);
                    foundIndices.Add(x.FirstLCN);

                    Console.CursorLeft = 0;
                    Console.Write("Found {0:N0} candidates, {1:N0} clusters remaining          ", filesToMove.Count, freeSpaceLeft);
                } while (freeSpaceLeft > 0);

                Console.WriteLine();

                if (filesToMove.Count == 0)
                {
                    FileInfoes firstFileAfterFreespace = extentInfos.OrderBy(s => s.FirstLCN).FirstOrDefault();
                    FileInfoes secondFileAfterFreespace = extentInfos.OrderBy(s => s.FirstLCN).Skip(1).FirstOrDefault();

                    // If it can fit between here, and the next file after the freespace. Move this.
                    ulong newFreespace = secondFileAfterFreespace.FirstLCN - freeSpace.Item1;

                    if (firstFileAfterFreespace.Extents.Length == 1 && newFreespace > firstFileAfterFreespace.TotalSize)
                    {
                        filesToMove.Add(firstFileAfterFreespace);
                    }
                    else
                        continue;
                }

                Console.WriteLine("Found {0:N0} items totalling {1:N0} clusters starting at LCN: {2:N0}", filesToMove.Count, filesToMove.Sum(s => s.TotalSize), filesToMove.Min(s => s.FirstLCN));
                Console.Write("Moves left: {0:N0}", filesToMove.Count);

                ulong currentLcn = freeSpace.Item1;
                for (int index = 0; index < filesToMove.Count; index++)
                {
                    FileInfoes fileInfoese = filesToMove[index];
                    try
                    {
                        using (SafeFileHandle fileHandle = CreateFile(fileInfoese.FilePath, FILE_READ_ATTRIBUTES,
                                                                   FileShare.ReadWrite, IntPtr.Zero, FileMode.Open,
                                                                   FileAttributes.Normal, IntPtr.Zero))
                        {
                            driveDeviceIo.FileSystemMoveFile(fileHandle.DangerousGetHandle(), 0, currentLcn,
                                                             fileInfoese.TotalSize);
                        }

                        Console.CursorLeft = 0;
                        Console.Write("Moves left: {0,-20:N0}", filesToMove.Count - index - 1);
                    }
                    catch (Win32Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(@"Could not place files: {0}", ex.Message);
                        continue;
                    }

                    // Update bitmap
                    foreach (FileExtentInfo extent in fileInfoese.Extents)
                    {
                        // Mark each extent as freed
                        for (ulong i = extent.Lcn; i < extent.Lcn + extent.Size; i++)
                        {
                            bitmap.Buffer[(int)i] = false;
                        }
                    }

                    for (ulong i = currentLcn; i < currentLcn + fileInfoese.TotalSize; i++)
                    {
                        // Mark as used
                        bitmap.Buffer[(int)i] = true;
                    }

                    // Update file extent
                    fileInfoese.Extents = new FileExtentInfo[1];
                    fileInfoese.Extents[0] = new FileExtentInfo
                        {
                            Lcn = currentLcn,
                            Size = fileInfoese.TotalSize,
                            Vcn = 0
                        };
                    fileInfoese.FirstLCN = currentLcn;

                    // Update pointer for next file
                    currentLcn += fileInfoese.TotalSize;
                }

                Console.Write("                                                        ");
                Console.CursorLeft = 0;

                somethingDone = true;
                diskPointer = currentLcn;
            } while (somethingDone);

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
        private static List<Tuple<ulong, ulong>> FindFreeSpaces(VOLUME_BITMAP_BUFFER bitmap)
        {
            List<Tuple<ulong, ulong>> freeSpaces = new List<Tuple<ulong, ulong>>();

            ulong lastFreeStart = 0;
            for (ulong i = 1; i < bitmap.BitmapSize; i++)
            {
                if (bitmap.Buffer[(int)(i - 1)] && !bitmap.Buffer[(int)i])
                {
                    // Switching from used to free
                    lastFreeStart = i;
                }
                else if (!bitmap.Buffer[(int)(i - 1)] && bitmap.Buffer[(int)i])
                {
                    // Switching from free to used
                    freeSpaces.Add(new Tuple<ulong, ulong>(lastFreeStart, i - lastFreeStart));
                }
            }

            return freeSpaces;
        }

        private static Tuple<ulong, ulong> FindFirstFreeSpace(VOLUME_BITMAP_BUFFER bitmap, ulong start)
        {
            start = start == 0 ? 1 : start;

            ulong lastFreeStart = 0;
            for (ulong i = start; i < bitmap.BitmapSize; i++)
            {
                if (bitmap.Buffer[(int)(i - 1)] && !bitmap.Buffer[(int)i])
                {
                    // Switching from used to free
                    lastFreeStart = i;
                }
                else if (!bitmap.Buffer[(int)(i - 1)] && bitmap.Buffer[(int)i] && lastFreeStart > start)
                {
                    // Switching from free to used
                    return new Tuple<ulong, ulong>(lastFreeStart, i - lastFreeStart);
                }
            }

            return null;
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
            FirstLCN = extents.Any() ? extents.Min(s => s.Lcn) : 0;
            TotalSize = extents.Any() ? (uint)extents.Sum(s => (uint)s.Size) : 0;
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
