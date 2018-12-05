using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;

namespace MobileManager.Controllers
{
    /// <summary>
    /// Appium log controller.
    /// </summary>
    [Route("api/v1/appium/log/")]
    [EnableCors("AllowAllHeaders")]
    public class AppiumLogController : ControllerExtensions, IAppiumLogController
    {
        private readonly IRestClient _restClient;
        private readonly IManagerLogger _logger;


        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.AppiumLogController" /> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="logger"></param>
        public AppiumLogController(IRestClient restClient, IManagerLogger logger) : base(logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Delete the appium log by device identifier.
        /// </summary>
        /// <returns>Tnull</returns>
        /// <param name="id">Device Identifier.</param>
        /// <response code="200">Appium log deleted successfully.</response>
        /// <response code="404">Appium log or device id not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            LogRequestToDebug();

            var device = await _restClient.GetDevice(id);
            if (device == null)
            {
                return NotFoundExtension("DeviceId not found in database.");
            }

            String appiumLogPath = await GetAppiumLogFilePath(id);

            if (!System.IO.File.Exists(appiumLogPath))
            {
                return NotFoundExtension("Appium log not found on path:" + appiumLogPath);
            }

            try
            {
                System.IO.File.Delete(appiumLogPath);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to delete AppiumLog. " + ex.Message);
            }

            _logger.Debug(string.Format("Successfully deleted appiumLog on path: [{0}]", appiumLogPath));

            return OkExtension();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the appium log by device identifier.
        /// </summary>
        /// <returns>Appium log.</returns>
        /// <param name="id">Device Identifier.</param>
        /// <response code="200">Appium log returned successfully.</response>
        /// <response code="404">Appium log or device id not found.</response>
        [HttpGet("{id}", Name = "getAppiumLog")]
        public async Task<IActionResult> GetById(string id)
        {
            LogRequestToDebug();

            var device = await _restClient.GetDevice(id);
            if (device == null)
            {
                return NotFoundExtension("DeviceId not found in database.");
            }

            String appiumLogPath = await GetAppiumLogFilePath(id);

            if (!System.IO.File.Exists(appiumLogPath))
            {
                return NotFoundExtension("Appium log not found on path:" + appiumLogPath);
            }

            var log = System.IO.File.ReadAllText(appiumLogPath);

            return JsonExtension(log);
        }

        private async Task<string> GetAppiumLogFilePath(string deviceId)
        {
            string appiumLogPath;

            var configuration = await _restClient.GetManagerConfiguration();


            if (configuration.AppiumLogFilePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                appiumLogPath = configuration.AppiumLogFilePath + deviceId + ".log";
            }
            else
            {
                appiumLogPath = configuration.AppiumLogFilePath + Path.DirectorySeparatorChar + deviceId + ".log";
            }

            if (appiumLogPath.StartsWith("~", StringComparison.Ordinal))
            {
                var homeDir = Environment.GetEnvironmentVariable("userprofile");
                if (String.IsNullOrEmpty(homeDir))
                {
                    homeDir = Environment.GetEnvironmentVariable("HOME");
                }
                appiumLogPath = appiumLogPath.Replace("~", homeDir);
            }

            return appiumLogPath;
        }
    }
}
