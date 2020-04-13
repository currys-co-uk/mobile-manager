using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Services.Interfaces;

#pragma warning disable 1998

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IAppResourceController" />
    /// <summary>
    /// App resources controller.
    /// </summary>
    [Route("api/v1/appPackage")]
    [EnableCors("AllowAllHeaders")]
    public class AppResourceController : ControllerExtensions, IAppResourceController
    {
        private readonly IRestClient _restClient;
        private readonly IManagerLogger _logger;
        private readonly IAppResourceService _appResourceService;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.AppResourceController" /> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="appResourceService">AppResourceService.</param>
        /// <param name="logger">Logger.</param>
        public AppResourceController(IRestClient restClient, IAppResourceService appResourceService, IManagerLogger logger) : base(logger)
        {
            _restClient = restClient;
            _logger = logger;
            _appResourceService = appResourceService;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the App resource info by hash identifier.
        /// </summary>
        /// <returns>The App resource by hash identifier.</returns>
        /// <param name="id">Id hash.</param>
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            LogRequestToDebug();
            var appInfo = _appResourceService.FindAppResource(id);
            
            if (appInfo == null)
            {
                return NotFoundExtension("Application resource not found on storage.");
            }

            return JsonExtension(appInfo);
        }

        /// <inheritdoc />
        /// <summary>
        /// Uploads from local storage APK/IPA source file for testing native applications
        /// </summary>
        /// <returns>Information about uploaded file</returns>
        /// <param name="file">Source file.</param>
        /// <response code="200">Application uploaded successfully</response>
        /// <response code="400">Invalid upload request</response>
        /// <response code="500">Internal failure</response>
        [HttpPost("upload")]
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            LogRequestToDebug();
            if (file == null || file.Length == 0)
                return BadRequestExtension("File not selected");

            var extension = Path.GetExtension(file.FileName).Trim('.').ToLower();
            if (extension != "ipa" && extension != "apk")
                return BadRequestExtension("File extension has to be IPA or APK");

            return JsonExtension(await _appResourceService.UploadAppResource(file));
        }

        /// <inheritdoc />
        /// <summary>
        /// Download from URL APK/IPA resource file for testing native applications
        /// </summary>
        /// <returns>Information about downloaded file</returns>
        /// <param name="uri">URI to resource file.</param>
        /// <response code="200">Application downloaded successfully</response>
        /// <response code="400">Invalid download request</response>
        /// <response code="500">Internal failure</response>
        [HttpPost("downloadFrom")]
        public IActionResult OnPostDownloadedAsync(Uri uri)
        {
            LogRequestToDebug();
            var extension = Path.GetExtension(uri.AbsolutePath).Trim('.').ToLower();
            if (extension != "ipa" && extension != "apk")
                return BadRequestExtension("File extension has to be IPA or APK");

            return JsonExtension(_appResourceService.DownloadAppResourceFromUri(uri));

        }

        /// <inheritdoc />
        /// <summary>
        /// Delete the specified appliacation resource by hash id.
        /// </summary>
        /// <param name="id">Id - application resource hash.</param>
        /// <response code="200">Application resource deleted successfully.</response>
        /// <response code="404">Application resource not found</response>
        /// <response code="500">Internal failure.</response>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            LogRequestToDebug();
            var appInfo = _appResourceService.FindAppResource(id);

            if (appInfo == null)
            {
                return NotFoundExtension("Application source not found on storage.");
            }

            _appResourceService.DeleteAppResource(id);
            return OkExtension(String.Format("App source successfully deleted: [{0}]", id));
        }
    }
}
