using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.App;
using Newtonsoft.Json;

#pragma warning disable 1998

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IAppPackageController" />
    /// <summary>
    /// Reservations queue controller.
    /// </summary>
    [Route("api/v1/appPackage")]
    [EnableCors("AllowAllHeaders")]
    public class AppPackageController : ControllerExtensions, IAppPackageController
    {
        private readonly IRestClient _restClient;
        private readonly IManagerLogger _logger;
        private readonly string StoragePath;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.AppPackageController" /> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="logger">Logger.</param>
        public AppPackageController(IRestClient restClient, IManagerLogger logger) : base(logger)
        {
            _restClient = restClient;
            _logger = logger;
            StoragePath = Path.Combine(Directory.GetCurrentDirectory(), "../_appFiles");
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the App source info by hash identifier.
        /// </summary>
        /// <returns>The App source by hash identifier.</returns>
        /// <param name="hash">Hash identifier.</param>
        [HttpGet("{hash}")]
        public IActionResult GetByHash(string hash)
        {
            LogRequestToDebug();
            AppResourceInfo info = this.GetAppInfoByHash(hash);
            
            if (info.Hash == null)
            {
                return NotFoundExtension("Application source not found on storage.");
            }

            return JsonExtension(info);
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

            if (!Directory.Exists(this.StoragePath))
            {
                Directory.CreateDirectory(this.StoragePath);
            }

            string hash = this.GetHashFromFile(file.OpenReadStream());
            var folderPath = Path.Combine(this.StoragePath, hash);
            var filePath = Path.Combine(folderPath, file.FileName);
            var fileInfoPath = Path.Combine(folderPath, "info.dat");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                using (var newStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(newStream);
                    var appInfo = new AppResourceInfo(hash, file.FileName, filePath, extension, DateTime.Now);
                    System.IO.File.WriteAllText(fileInfoPath, JsonConvert.SerializeObject(appInfo));
                }
            }

            return this.GetByHash(hash);
        }

        /// <inheritdoc />
        /// <summary>
        /// Download from URL APK/IPA source file for testing native applications
        /// </summary>
        /// <returns>Information about downloaded file</returns>
        /// <param name="uri">URI to source file.</param>
        /// <response code="200">Application downloaded successfully</response>
        /// <response code="400">Invalid download request</response>
        /// <response code="500">Internal failure</response>
        [HttpPost("downloadFrom")]
        public async Task<IActionResult> OnPostDownloadedAsync(Uri uri)
        {
            LogRequestToDebug();
            var extension = Path.GetExtension(uri.AbsolutePath).Trim('.').ToLower();
            if (extension != "ipa" && extension != "apk")
                return BadRequestExtension("File extension has to be IPA or APK");

            /* 
             * at first try HEAD HTTP request
             * if is MD5 there we can use it without necessity to download whole file 
             * It works for e.g. artifactory 
             */
            WebRequest request = WebRequest.Create(uri);
            request.Method = "HEAD";
            var md5 = request.GetResponse().Headers.Get("X-Checksum-Md5");
            if (md5 != null)
            {
                var appResourceInfo = this.GetAppInfoByHash(md5);
                if (appResourceInfo.Hash == md5)
                {
                    return JsonExtension(appResourceInfo);
                } 
            }

            string fileName = Path.GetFileName(uri.AbsolutePath);
            string randomTemp = "_temp" + (new Random()).Next(10000);
            var tempFolderPath = Path.Combine(this.StoragePath, randomTemp);
            var tempFilePath = Path.Combine(tempFolderPath, fileName);

            using (var client = new WebClient())
            {
                Directory.CreateDirectory(tempFolderPath);
                client.DownloadFile(uri, tempFilePath);
            }

            var fileStream = new FileStream(tempFilePath, FileMode.Open);
            string hash = this.GetHashFromFile(fileStream);
            fileStream.Close();
            var appFound = this.GetAppInfoByHash(hash);

            if (appFound.Hash == hash)
            {
                // already downloaded - delete temp folder, return past
                Directory.Delete(tempFolderPath, true);
                return JsonExtension(appFound);
            }
            
            // new download - just rename temp folder and generate info file
            var folderPath = Path.Combine(this.StoragePath, hash);
            var filePath = Path.Combine(folderPath, fileName);
            var fileInfoPath = Path.Combine(folderPath, "info.dat");
            Directory.Move(tempFolderPath, folderPath);

            var appInfo = new AppResourceInfo(hash, fileName, filePath, extension, DateTime.Now);
            System.IO.File.WriteAllText(fileInfoPath, JsonConvert.SerializeObject(appInfo));

            return JsonExtension(appInfo);

        }

        /// <inheritdoc />
        /// <summary>
        /// Delete the specified appliacation source by hash.
        /// </summary>
        /// <returns>null.</returns>
        /// <param name="hash">Application source hash.</param>
        /// <response code="200">Application source deleted successfully.</response>
        /// <response code="404">Application source not found</response>
        /// <response code="500">Internal failure.</response>
        [HttpDelete("{hash}")]
        public IActionResult Delete(string hash)
        {
            LogRequestToDebug();
            var appFolder = Path.Combine(this.StoragePath, hash);

            if (!Directory.Exists(appFolder))
            {
                return NotFoundExtension("Application source not found on storage.");
            }

            Directory.Delete(appFolder, true);
            return OkExtension(String.Format("App source successfully deleted: [{0}]", hash));
        }

        private AppResourceInfo GetAppInfoByHash(string hash)
        {
            var appInfoFile = Path.Combine(this.StoragePath, hash, "info.dat");

            if (!System.IO.File.Exists(appInfoFile))
            {
                return new AppResourceInfo();
            }

            return JsonConvert.DeserializeObject<AppResourceInfo>(System.IO.File.ReadAllText(appInfoFile));
        }

        private string GetHashFromFile(Stream fileStream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
