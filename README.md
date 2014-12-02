DeviceIOControlLib
==================
A C# Library to work with the Win32 DeviceIOControl method

Reasons
-------

I wanted to make a program in C#, that could read the Raw sectors on any attached hard drive. It turns out this was really simple, except, to make it portable / work for any drive, one needed to know the Sector size. 

That in turn - was not simple. I then discovered the Windows device control method and its associated Control Codes. I then found what I believe to be the single method with the most functionality in Win32. The DeviceIOControl Win32 call has, from what I can count, at least 150-200 known uses, and reserved space for thousands more.

It also turns out, that the amount of help for this function on the internet for use in C# is ... close to non-existent. So I decided to try and map at least some of the DeviceIOControl functionalities in a library.


----------
Basic idea
----------

The basic idea, is to take a handle to a Windows device (e.g. a hard drive, CD-ROM drive, tape drive, disk changer, USB device, Bluetooth device, file system ... ), and then make it easy to send control commands to this device.

So I made a wrapper, that does this.

----------
How to use - simple use scenarios
----------

**Opening and closing CD-ROM trays**

A simple scenario is to open the CD-ROM tray. First we need a handle to the CD-ROM device. A great tool to discover devices is the [Sysinternals Winobj][1] program. My CD-ROM drive is named: "\\.\CdRom0". I then use the [Win32 CreateFile()][2] to open the device. I do this with Win32 instead of FileStream as FileStream doesn't support devices - sadly.

    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using DeviceIOControlLib;
    using Microsoft.Win32.SafeHandles;
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
            SafeFileHandle safeHandle = CreateFile(@"\\.\CdRom0", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            DeviceIOControlWrapper deviceIo = new DeviceIOControlWrapper(safeHandle);

            // Open tray
            deviceIo.StorageEjectMedia();

            Thread.Sleep(TimeSpan.FromSeconds(10));

            // Close tray
            deviceIo.StorageLoadMedia();
        }
    }

In the code above, we have all the boilerplate as well (such as the CreateFile DLL import from [pinvoke.net][3]). After we get the handle, we use it in the DeviceIOControlWrapper, and simple call some Storage methods on it. For CD drives, we can eject and 'load' media. I do that, and voilà, CD tray opens and closes.

----------

**Reading out information about hard drives**

The thing that got me started, was reading out sector sizes. So, let’s do that. Working from code above, we open a handle to PhysicalDriveN instead (all hard drives attached are mapped this way).

    SafeFileHandle safeHandle = CreateFile(@"\\.\PhysicalDrive0", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

PhysicalDrive0 is, of course, the first drive (most often the drive C:\ is on). When we have the handle, we can get the drive Geometry from it:

    DISK_GEOMETRY_EX info = deviceIo.DiskGetDriveGeometryEx();

This structure (DISK\_GEOMETRY\_EX) contains all the info directly from the Win32 method. Namely this piece:

    Console.WriteLine("Sector size: " + info.Geometry.BytesPerSector);

----------

Improvement / Design / Future
--------------------

The library is nowhere near complete, and never will be, as all driver developers are free to add their own control codes. What I intend to do, is create a baseline where the most commonly used control codes (like those Microsoft make available) are implemented. I've made it possible to call the DeviceIOControl method directly (without having to map the function in the library), so you may use it as you please.

As a design guideline, I intend to put all structures, enums and such in a file called "DeviceIO*.cs" where '*' is the name of the device type. E.g. all Disk structures and enums are in "DeviceIODisk.cs". The methods themselves are implemented in DeviceIoControlWrapper.cs

I also try best possible, to link to a page describing the details and quirks of each method. E.g.

    /// <summary>
    /// Used to open/eject CD-ROM trays
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa363406(v=vs.85).aspx" />
    /// </summary>
    public bool StorageEjectMedia()
    {
    	return InvokeIoControl(Handle, IOControlCode.StorageEjectMedia);
    }

  [1]: http://technet.microsoft.com/en-us/sysinternals/bb896657.aspx
  [2]: http://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
  [3]: http://www.pinvoke.net/default.aspx/kernel32.createfile