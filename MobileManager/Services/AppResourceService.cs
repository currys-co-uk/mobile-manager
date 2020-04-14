using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MobileManager.Controllers;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.App;
using MobileManager.Services.Interfaces;

namespace MobileManager.Services
{
    /// <summary>
    /// AppResource service.
    /// </summary>
    public class AppResourceService : ControllerExtensions, IAppResourceService
    {
        private readonly IManagerLogger _logger;
        private readonly IRestClient _restClient;
        private readonly IRepository<AppResourceInfo> _appResourceRepository;
        private readonly string _storagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Services.AppResourceService"/> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="appResourceRepository">appResourceRepository.</param>
        /// <param name="logger">Logger.</param>
        public AppResourceService(IRestClient restClient, IRepository<AppResourceInfo> appResourceRepository, IManagerLogger logger) : base(logger)
        {
            _logger = logger;
            _restClient = restClient;
            _appResourceRepository = appResourceRepository;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "../_appFiles");
        }

        /// <inheritdoc />
        public AppResourceInfo FindAppResource(string id)
        {
            return _appResourceRepository.Find(id);
        }

        /// <inheritdoc />
        public bool DeleteAppResource(string id)
        {
            var appFolder = Path.Combine(_storagePath, id);
            var result = _appResourceRepository.Remove(id);
            Directory.Delete(appFolder, true);
            return true;
        }

        /// <inheritdoc />
        public async Task<AppResourceInfo> UploadAppResource(IFormFile file)
        {
            CreateStorageFolder();

            var extension = Path.GetExtension(file.FileName).Trim('.').ToLower();
            var hash = GetHashFromFile(file.OpenReadStream());
            var folderPath = Path.Combine(_storagePath, hash);
            var filePath = Path.Combine(folderPath, file.FileName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                var newStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(newStream);
                var appInfo = new AppResourceInfo(hash, file.FileName, filePath, extension, DateTime.Now);
                _appResourceRepository.Add(appInfo);
                newStream.Close();
                return appInfo;
            }

            return FindAppResource(hash);
        }

        /// <inheritdoc />
        public AppResourceInfo DownloadAppResourceFromUri(Uri uri)
        {
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
                var appResourceInfo = FindAppResource(md5);
                if (appResourceInfo.Id == md5)
                {
                    return appResourceInfo;
                }
            }

            var fileName = Path.GetFileName(uri.AbsolutePath);
            var randomTemp = "_temp" + (new Random()).Next(10000);
            var tempFolderPath = Path.Combine(_storagePath, randomTemp);
            var tempFilePath = Path.Combine(tempFolderPath, fileName);

            using (var client = new WebClient())
            {
                Directory.CreateDirectory(tempFolderPath);
                client.DownloadFile(uri, tempFilePath);
            }

            var fileStream = new FileStream(tempFilePath, FileMode.Open);
            var hash = GetHashFromFile(fileStream);
            fileStream.Close();
            var appFound = FindAppResource(hash);

            if (appFound != null && appFound.Id == hash)
            {
                // already downloaded - delete temp folder, return past
                Directory.Delete(tempFolderPath, true);
                return appFound;
            }

            // new download - just rename temp folder and generate info file
            var folderPath = Path.Combine(_storagePath, hash);
            var filePath = Path.Combine(folderPath, fileName);
            var extension = Path.GetExtension(uri.AbsolutePath).Trim('.').ToLower();

            Directory.Move(tempFolderPath, folderPath);
            var appInfo = new AppResourceInfo(hash, fileName, filePath, extension, DateTime.Now);
            _appResourceRepository.Add(appInfo);

            return appInfo;
        }

        #region privateMethods

        private string GetHashFromFile(Stream fileStream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
            }
        }

        private void CreateStorageFolder () 
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        #endregion
    }
}
