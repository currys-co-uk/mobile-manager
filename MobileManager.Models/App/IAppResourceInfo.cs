using System.ComponentModel.DataAnnotations;

namespace MobileManager.Models.App
{
    /// <summary>
    /// App resource file info.
    /// </summary>
    public interface IAppResourceInfo
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        [Key]
        string Id { get; set; }

        /// <summary>
        /// Gets the FileName
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets the whole file path of app.
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Gets extension/type of file
        /// </summary>
        string FileExtension { get; set; }

        /// <summary>
        /// Gets original upload DateTime of file
        /// </summary>
        System.DateTime UploadTime { get; set; }
    }
}
