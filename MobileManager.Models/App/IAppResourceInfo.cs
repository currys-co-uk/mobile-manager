namespace MobileManager.Models.App
{
    /// <summary>
    /// App resource file info.
    /// </summary>
    public interface IAppResourceInfo
    {
        /// <summary>
        /// The FileName
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets the hash of app.
        /// </summary>
        string Hash { get; set; }

        /// <summary>
        /// Gets the whole file path of app.
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Gets extension/type of file
        /// </summary>
        string FileExtension { get; set; }

        /// <summary>
        /// Gets original upload time of file
        /// </summary>
        System.DateTime UploadTime { get; set; }
    }
}
