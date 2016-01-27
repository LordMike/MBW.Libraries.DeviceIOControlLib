using System.ComponentModel;

namespace DeviceIOControlLib.Utilities
{
    internal static class Utils
    {
        public static string GetWin32ErrorMessage(int errorCode)
        {
            return new Win32Exception(errorCode).Message;
        }
    }
}
