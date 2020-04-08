namespace MobileManager.Models.App
{
    /// <inheritdoc />
    /// <summary>
    /// App resource file info.
    /// </summary>
    public class AppResourceInfo : IAppResourceInfo
    {
        public AppResourceInfo(string hash, string fileName, string filePath, string fileExtension, System.DateTime uploadTime)
        {
            Hash = hash;
            FileName = fileName;
            FilePath = filePath;
            FileExtension = fileExtension;
            UploadTime = uploadTime;
        }

        public AppResourceInfo()
        {
        }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <inheritdoc />
        public string Hash { get; set; }

        /// <inheritdoc />
        public string FilePath { get; set; }

        /// <inheritdoc />
        public string FileExtension { get; set; }

        /// <inheritdoc />
        public System.DateTime UploadTime { get; set; }
    }
}
