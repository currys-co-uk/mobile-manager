using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Application package controller.
    /// </summary>
    public interface IAppPackageController
    {
        /// <summary>
        /// Gets information about existing or non-existing application hash on storage
        /// </summary>
        /// <returns>The by hash identifier.</returns>
        /// <param name="hash">Identifier.</param>
        [HttpGet("{hash}")]
        IActionResult GetByHash(string hash);

        /// <summary>
        /// Upload the file the async.
        /// </summary>
        [HttpPost("upload")]
        Task<IActionResult> OnPostUploadAsync(IFormFile file);

        /// <summary>
        /// Download the file the async.
        /// </summary>
        [HttpPost("downloadFrom")]
        Task<IActionResult> OnPostDownloadedAsync(Uri uri);

        /// <summary>
        /// Delete the specified id.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="hash">Identifier.</param>
        [HttpDelete("{hash}")]
        IActionResult Delete(string hash);
    }
}