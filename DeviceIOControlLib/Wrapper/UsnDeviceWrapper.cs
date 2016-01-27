using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.FileSystem;
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
        //FsctlReadUsnJournal
        //FsctlWriteUsnCloseRecord

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
                        // Get record length
                        int length = Marshal.ReadInt32(mem, dataOffset);
                        int majorVersion = Marshal.ReadByte(mem, dataOffset + sizeof(int)) + (Marshal.ReadByte(mem, dataOffset + sizeof(int) + 1) << 8);

                        if (length <= 0)
                            // No more records
                            break;

                        // Copy out record subset
                        switch (majorVersion)
                        {
                            case 2:
                                USN_RECORD_V2 recordv2 = (USN_RECORD_V2)Marshal.PtrToStructure(mem.Handle + dataOffset, typeof(USN_RECORD_V2));

                                // Parse string manually, as we cannot rely on the string to be null-terminated.
                                recordv2.FileName = Marshal.PtrToStringUni(mem.Handle + dataOffset + recordv2.FileNameOffset, recordv2.FileNameLength / 2);

                                res.Add(recordv2);

                                break;
                            case 3:
                                USN_RECORD_V3 recordv3 = (USN_RECORD_V3)Marshal.PtrToStructure(mem.Handle + dataOffset, typeof(USN_RECORD_V3));

                                // Parse string manually, as we cannot rely on the string to be null-terminated.
                                recordv3.FileName = Marshal.PtrToStringUni(mem.Handle + dataOffset + recordv3.FileNameOffset, recordv3.FileNameLength / 2);

                                res.Add(recordv3);

                                break;
                            default:
                                // Ignore
                                Debugger.Break();
                                break;
                        }

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