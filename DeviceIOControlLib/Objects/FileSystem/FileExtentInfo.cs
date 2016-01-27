namespace DeviceIOControlLib.Objects.FileSystem
{
    public class FileExtentInfo
    {
        /// <summary>
        /// Position in file
        /// </summary>
        public ulong Vcn { get; set; }

        /// <summary>
        /// Position on disk
        /// </summary>
        public ulong Lcn { get; set; }

        /// <summary>
        /// Size in # of clusters
        /// </summary>
        public ulong Size { get; set; }
    }
}