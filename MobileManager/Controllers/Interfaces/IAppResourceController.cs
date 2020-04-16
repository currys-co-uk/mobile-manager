using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Application resource controller.
    /// </summary>
    public interface IAppResourceController
    {
        /// <summary>
        /// Gets the App resource info by hash identifier.
        /// </summary>
        [HttpGet("{id}")]
        IActionResult GetById(string id);

        /// <summary>
        /// Uploads from local storage APK/IPA source file for testing native applications
        /// </summary>
        [HttpPost("upload")]
        Task<IActionResult> OnPostUploadAsync(IFormFile file);

        /// <summary>
        /// Download from URL APK/IPA resource file for testing native applications
        /// </summary>
        [HttpPost("downloadFrom")]
        IActionResult OnPostDownloadedAsync(Uri uri);

        /// <summary>
        /// Delete the specified appliacation resource by hash id.
        /// </summary>
        [HttpDelete("{id}")]
        IActionResult Delete(string id);
    }
}