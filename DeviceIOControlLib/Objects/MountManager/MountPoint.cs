namespace DeviceIOControlLib.Objects.MountManager
{
    public class MountPoint
    {
        public string SymbolicLinkName { get; set; }
        public byte[] UniqueId { get; set; }
        public string DeviceName { get; set; }
    }
}