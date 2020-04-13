using Microsoft.AspNetCore.Http;
using MobileManager.Models.App;
using System;
using System.Threading.Tasks;

namespace MobileManager.Services.Interfaces
{
    /// <summary>
    /// AppResource service.
    /// </summary>
    public interface IAppResourceService
    {
        /// <summary>
        /// Finds App resource info by hash id
        /// </summary>
        /// <returns>AppResourceInfo.</returns>
        /// <param name="id">Id.</param>
        AppResourceInfo FindAppResource(string id);

        /// <summary>
        /// Deletes App resource info by hash id
        /// </summary>
        /// <returns>Boolean of success</returns>
        /// <param name="id">Id.</param>
        bool DeleteAppResource(string id);

        /// <summary>
        /// Uploads the App resource file from IFormFile request.
        /// </summary>
        /// <returns>App resource info.</returns>
        /// <param name="file">File.</param>
        Task<AppResourceInfo> UploadAppResource(IFormFile file);

        /// <summary>
        /// Downloads the App resource file from URI.
        /// </summary>
        /// <returns>App resource info.</returns>
        /// <param name="uri">URI.</param>
        AppResourceInfo DownloadAppResourceFromUri(Uri uri);
    }
}
