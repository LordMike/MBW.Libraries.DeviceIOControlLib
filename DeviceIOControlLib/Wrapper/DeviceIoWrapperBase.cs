using System;
using Microsoft.Win32.SafeHandles;

namespace DeviceIOControlLib.Wrapper
{
    public abstract class DeviceIoWrapperBase : IDisposable
    {
        private readonly bool _ownsHandle;

        public SafeFileHandle Handle { get; }

        public DeviceIoWrapperBase(SafeFileHandle handle, bool ownsHandle = false)
        {
            if (handle.IsInvalid)
                throw new ArgumentException("Handle is invalid");

            _ownsHandle = ownsHandle;
            Handle = handle;
        }

        public void Close()
        {
            if (Handle != null && !Handle.IsClosed)
                Handle.Close();
        }

        public void Dispose()
        {
            if (_ownsHandle)
                Close();
        }
    }
}