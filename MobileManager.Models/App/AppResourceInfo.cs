using System;
using System.ComponentModel.DataAnnotations;

namespace MobileManager.Models.App
{
    /// <inheritdoc />
    /// <summary>
    /// App resource file info.
    /// </summary>
    public class AppResourceInfo : IAppResourceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.App.AppResourceInfo"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="fileName">FileName.</param>
        /// <param name="filePath">FilePath</param>
        /// <param name="fileExtension">FileExtension.</param>
        /// <param name="uploadTime">UploadTime.</param>
        public AppResourceInfo(string id, string fileName, string filePath, string fileExtension, DateTime uploadTime)
        {
            Id = id;
            FileName = fileName;
            FilePath = filePath;
            FileExtension = fileExtension;
            UploadTime = uploadTime;
        }

        /// <inheritdoc />
        [Key]
        public string Id { get; set; }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <inheritdoc />
        public string FilePath { get; set; }

        /// <inheritdoc />
        public string FileExtension { get; set; }

        /// <inheritdoc />
        public DateTime UploadTime { get; set; }
    }
}
