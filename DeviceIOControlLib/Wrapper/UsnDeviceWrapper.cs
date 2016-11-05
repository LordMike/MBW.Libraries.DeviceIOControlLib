using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.FileSystem;
using DeviceIOControlLib.Objects.Usn;
using DeviceIOControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public class UsnDeviceWrapper : DeviceIoWrapperBase
    {
        public UsnDeviceWrapper(SafeFileHandle handle, bool ownsHandle = false)
            : base(handle, ownsHandle)
        {
        }

        //FsctlCreateUsnJournal
        //FsctlDeleteUsnJournal
        //FsctlReadFileUsnData
        //FsctlWriteUsnCloseRecord

        /// <summary><see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364586(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemReadUsnJournal(UsnJournalReasonMask reasonMask, int bytesToWaitFor = 0, int timeout = 0)
        {
            USN_JOURNAL_DATA_V0 usnQuery = FileSystemQueryUsnJournal();

            return FileSystemReadUsnJournal(usnQuery.UsnJournalID, reasonMask, bytesToWaitFor, timeout);
        }

        /// <summary><see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364586(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemReadUsnJournal(long volumeJournalId, UsnJournalReasonMask reasonMask, int bytesToWaitFor = 0, int timeout = 0)
        {
            return FileSystemReadUsnJournal(volumeJournalId, reasonMask, new USN(), bytesToWaitFor, timeout);
        }

        /// <summary><see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364586(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemReadUsnJournal(UsnJournalReasonMask reasonMask, USN firstUsn, int bytesToWaitFor = 0, int timeout = 0)
        {
            USN_JOURNAL_DATA_V0 usnQuery = FileSystemQueryUsnJournal();

            return FileSystemReadUsnJournal(usnQuery.UsnJournalID, reasonMask, firstUsn, bytesToWaitFor, timeout);
        }

        private static IUSN_RECORD ParseUsnRecord(UnmanagedMemory mem, int dataOffset, out int length)
        {
            // Get record length
            length = Marshal.ReadInt32(mem, dataOffset);
            int majorVersion = Marshal.ReadByte(mem, dataOffset + sizeof(int)) + (Marshal.ReadByte(mem, dataOffset + sizeof(int) + 1) << 8);

            if (length <= 0)
                // No more records
                return null;

            // Copy out record subset
            switch (majorVersion)
            {
                case 2:
                    USN_RECORD_V2 recordv2 = new IntPtr(mem.Handle.ToInt64() + dataOffset) .ToStructure<USN_RECORD_V2>();

                    // Parse string manually, as we cannot rely on the string to be null-terminated.
                    recordv2.FileName = Marshal.PtrToStringUni(new IntPtr(mem.Handle.ToInt64() + dataOffset + recordv2.FileNameOffset), recordv2.FileNameLength / 2);

                    return recordv2;
                case 3:
                    USN_RECORD_V3 recordv3 = new IntPtr(mem.Handle.ToInt64() + dataOffset).ToStructure<USN_RECORD_V3>();

                    // Parse string manually, as we cannot rely on the string to be null-terminated.
                    recordv3.FileName = Marshal.PtrToStringUni(new IntPtr(mem.Handle.ToInt64() + dataOffset + recordv3.FileNameOffset), recordv3.FileNameLength / 2);

                    return recordv3;
                default:
                    // Ignore
                    break;
            }

            return null;
        }

        /// <summary><see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364586(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemReadUsnJournal(long volumeJournalId, UsnJournalReasonMask reasonMask, USN firstUsn, int bytesToWaitFor = 0, int timeout = 0)
        {
            READ_USN_JOURNAL_DATA_V0 input = new READ_USN_JOURNAL_DATA_V0();

            input.StartUsn = firstUsn;
            input.UsnJournalId = volumeJournalId;
            input.ReasonMask = reasonMask;
            input.BytesToWaitFor = bytesToWaitFor;
            input.Timeout = timeout;

            int errorCode;
            byte[] data = DeviceIoControlHelper.InvokeIoControl(Handle, IOControlCode.FsctlReadUsnJournal, 1024 * 1024, input, out errorCode);

            List<IUSN_RECORD> res = new List<IUSN_RECORD>();
            using (UnmanagedMemory mem = new UnmanagedMemory(data))
            {
                int dataOffset = 8;

                while (dataOffset < data.Length)
                {
                    int length;
                    IUSN_RECORD rec = ParseUsnRecord(mem, dataOffset, out length);

                    if (length <= 0)
                        break;

                    res.Add(rec);
                    
                    // Move to next record
                    dataOffset += length;
                }
            }

            return res.ToArray();
        }

        /// <summary><see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa364563(v=vs.85).aspx"/></summary>
        public IUSN_RECORD[] FileSystemEnumUsnData()
        {
            ulong nextUsn = 0;
            const int chunkSize = 1 * 1024 * 1024;      // 1 MB chunks

            List<IUSN_RECORD> res = new List<IUSN_RECORD>();

            using (UnmanagedMemory mem = new UnmanagedMemory(chunkSize))
            {
                do
                {
                    MFT_ENUM_DATA_V0 input = new MFT_ENUM_DATA_V0();
                    input.StartFileReferenceNumber = nextUsn;
                    input.LowUsn = long.MinValue;
                    input.HighUsn = long.MaxValue;

                    int errorCode;
                    byte[] data = DeviceIoControlHelper.InvokeIoControl(Handle, IOControlCode.FsctlEnumUsnData, chunkSize, input, out errorCode);
                    Marshal.Copy(data, 0, mem, data.Length);

                    if (errorCode != 0)
                        // Exit when theres no more to do
                        break;

                    nextUsn = BitConverter.ToUInt64(data, 0);

                    int dataOffset = 8;

                    while (dataOffset < data.Length)
                    {
                        int length;
                        IUSN_RECORD rec = ParseUsnRecord(mem, dataOffset, out length);

                        if (length <= 0)
                            break;

                        res.Add(rec);

                        // Move to next record
                        dataOffset += length;
                    }

                    // Fetch next chunk
                } while (true);
            }

            return res.ToArray();
        }

        /// <summary><see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364583(v=vs.85).aspx"/></summary>
        public USN_JOURNAL_DATA_V0 FileSystemQueryUsnJournal()
        {
            USN_JOURNAL_DATA_V0 res = DeviceIoControlHelper.InvokeIoControl<USN_JOURNAL_DATA_V0>(Handle, IOControlCode.FsctlQueryUsnJournal);

            return res;
        }
    }
}