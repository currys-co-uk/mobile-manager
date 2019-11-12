using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers.Interfaces;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Devices.Interfaces;
using MobileManager.Services;
using MobileManager.Utils;
using MobileManager.SeleniumConfigs.DataSets;
using Newtonsoft.Json;
using DotLiquid;
using DotLiquid.FileSystems;

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IDeviceController" />
    /// <summary>
    /// Devices controller.
    /// </summary>
    [Route("api/v1/device")]
    [EnableCors("AllowAllHeaders")]
    public class DevicesController : ControllerExtensions, IDeviceController
    {
        private readonly IRepository<Device> _devicesRepository;
        private readonly IManagerLogger _logger;
        private readonly IManagerConfiguration _configuration;
        private readonly IScreenshotService _screenshotService;
        private readonly IDeviceUtils _deviceUtils;
        private readonly IExternalProcesses _externalProcesses;


        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.DevicesController" /> class.
        /// </summary>
        /// <param name="devicesRepository"><see cref="IRepository{T}"/> Device.</param>
        /// <param name="logger"><see cref="IManagerLogger"/></param>
        /// <param name="configuration"><see cref="IManagerConfiguration"/></param>
        /// <param name="screenshotService"><see cref="IScreenshotService"/></param>
        /// <param name="externalProcesses"><see cref="IExternalProcesses"/></param>
        /// <param name="deviceUtils"><see cref="IDeviceUtils"/></param>
        public DevicesController(IRepository<Device> devicesRepository, IManagerLogger logger,
            IManagerConfiguration configuration, IDeviceUtils deviceUtils,
            IScreenshotService screenshotService, IExternalProcesses externalProcesses) : base(logger)
        {
            _devicesRepository = devicesRepository;
            _logger = logger;
            _configuration = configuration;
            _deviceUtils = deviceUtils ?? new DeviceUtils(_logger, _externalProcesses);
            _screenshotService = screenshotService ?? new ScreenshotService(_logger, _externalProcesses);
            _externalProcesses = externalProcesses ?? new ExternalProcesses(_logger);
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all active devices.
        /// </summary>
        /// <returns>Active devices.</returns>
        [HttpGet]
        public IEnumerable<Device> GetAll()
        {
            LogRequestToDebug();

            var devices = _devicesRepository.GetAll();
            _logger.Debug(string.Format("GetAll devices: [{0}]", JsonConvert.SerializeObject(devices)));
            return devices;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets device the by identifier.
        /// </summary>
        /// <returns>The device identifier.</returns>
        /// <param name="id">Identifier.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="404">Device not found.</response>
        [HttpGet("{id}", Name = "getDevice")]
        public IActionResult GetById(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            return JsonExtension(device);
        }

        /// <summary>
        /// Gets device properties the by identifier.
        /// </summary>
        /// <returns>The device properties.</returns>
        /// <param name="id">Identifier.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="404">Device not found.</response>
        [HttpGet("properties/{id}", Name = "getDeviceProperties")]
        public IActionResult GetPropertiesById(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            return JsonExtension(device.Properties);
        }

        /// <summary>
        /// Gets device properties keys.
        /// </summary>
        /// <returns>The device property keys.</returns>
        /// <response code="200">Property keys returned successfully.</response>
        [HttpGet("properties", Name = "getDevicePropertiesKeys")]
        public IActionResult GetAllPropertiesKeys()
        {
            LogRequestToDebug();

            var devices = _devicesRepository.GetAll();
            var properties = new List<DeviceProperties>();

            foreach (var device in devices)
            {
                properties.AddRange(device.Properties);
            }

            var keys = properties.Select(p => p.Key).Distinct();

            return JsonExtension(keys);
        }

        /// <summary>
        /// Gets device properties the by identifier.
        /// </summary>
        /// <returns>The device properties.</returns>
        /// <param name="id">Identifier.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="404">Device not found.</response>
        [HttpGet("seleniumConfig/{id}", Name = "getDeviceSeleniumConfig")]
        public IActionResult GetSeleniumConfigById(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            switch (device.Type)
            {
                case DeviceType.IOS:
                    return JsonExtension(CreateIosSeleniumConfig(device));
                case DeviceType.Android:
                    return JsonExtension(CreateAndroidSeleniumConfig(device));
                case DeviceType.Unspecified:
                    return JsonExtension(CreateIosSeleniumConfig(device));
                default:
                    return JsonExtension("Unsupported device type");
            }
        }

        private string CreateIosSeleniumConfig(IDevice device)
        {
            string readText = System.IO.File.ReadAllText(@"SeleniumConfigTemplates/XTestIOS.tt");
            Template template = Template.Parse(readText); 

            IosSeleniumConfig data = new IosSeleniumConfig(device, _configuration);
            return template.Render(Hash.FromAnonymousObject(new {device = data}));
            
            
            var config = new StringBuilder();
            config.AppendLine($"[mobile-manager-{device.Id}]");
            config.AppendLine(device.AppiumEndpoint != string.Empty ? $"url = {device.AppiumEndpoint}" : $"url = ");
            config.AppendLine($"browserName = safari mobile");
            config.AppendLine($"platformName = iOS");
            config.AppendLine($"udid = {device.Id}");
            config.AppendLine($"deviceName = \"{device.Name}\"");
            config.AppendLine($"deviceVersion = {device.Properties.First(x => x.Key == "ProductVersion").Value}");
            config.AppendLine($"automationName = XCUITest");
            config.AppendLine($"teamId = {_configuration.IosDeveloperCertificateTeamId}");
            config.AppendLine($"signingId = \"iPhone Developer\"");
            config.AppendLine($"showXcodeLog: true");
            config.AppendLine($"realDeviceLogger = /usr/local/lib/node_modules/deviceconsole/deviceconsole");
            config.AppendLine($"bootstrapPath = /usr/local/lib/node_modules/appium/node_modules/appium-webdriveragent");
            config.AppendLine($"agentPath = /usr/local/lib/node_modules/appium/node_modules/appium-webdriveragent/WebDriverAgent.xcodeproj");
            config.AppendLine("sessionTimeout = 6000");
            config.AppendLine($"startIWDP: true");

            return config.ToString();
        }

        private static string CreateAndroidSeleniumConfig(IDevice device)
        {
            /*var config = new StringBuilder();*/

            string readText = System.IO.File.ReadAllText(@"SeleniumConfigTemplates/XTestAndroid.tt");
            Template template = Template.Parse(readText); 

            AndroidSeleniumConfig data = new AndroidSeleniumConfig(device);
            return template.Render(Hash.FromAnonymousObject(new {device = data}));

            /*config.AppendLine($"[mobile-manager-{device.Id}]");
            config.AppendLine(device.AppiumEndpoint != string.Empty ? $"url = {device.AppiumEndpoint}" : $"url = ");
            config.AppendLine($"browserName = chrome mobile");
            config.AppendLine($"platformName = android");
            config.AppendLine($"udid = {device.Id}");
            config.AppendLine($"deviceName = \"{device.Name}\" ");
            config.AppendLine($"sessionTimeout = 6000");
            config.AppendLine($"automationName = UiAutomator2");

            return config.ToString();*/
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the specified device.
        /// </summary>
        /// <returns>Created device.</returns>
        /// <param name="device">Device.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="400">Invalid device in request</response>
        /// <response code="409">Device already exists.</response>
        /// <response code="500">Internal failure.</response>
        [HttpPost]
        public IActionResult Create([FromBody] Device device)
        {
            LogRequestToDebug();

            if (device == null)
            {
                return BadRequestExtension("Empty device in request");
            }

            if (_devicesRepository.Find(device.Id) != null)
            {
                // 409 = Conflict
                return StatusCodeExtension(409, "Device ID already stored in database.");
            }

            if (string.IsNullOrEmpty(device.Id) || string.IsNullOrEmpty(device.Name))
            {
                return BadRequestExtension("Device Id and Name has to be specified.");
            }

            try
            {
                _devicesRepository.Add(device);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to Add device in database. " + ex.Message);
            }

            _logger.Debug(string.Format("Created new device: [{0}]", JsonConvert.SerializeObject(device)));

            return CreatedAtRoute("getDevice", new {id = device.Id}, device);
        }

        /// <inheritdoc />
        /// <summary>
        /// Update the specified device.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="deviceUpdated">Device updated.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="400">Invalid device in request</response>
        /// <response code="404">Device not found in database.</response>
        /// <response code="500">Internal failure.</response>
        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] Device deviceUpdated)
        {
            LogRequestToDebug();

            if (deviceUpdated == null || deviceUpdated.Id != id)
            {
                return BadRequestExtension("Empty device in request");
            }

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            //todo: why is this commented out
            /*
            if (!device.Available)
            {
                return StatusCodeExtension(423, "Device is locked.");
            }
            */

            try
            {
                _devicesRepository.Update(deviceUpdated);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to Update device in database. " + ex.Message);
            }

            _logger.Debug(
                $"Updated device: [{JsonConvert.SerializeObject(device)}] to [{JsonConvert.SerializeObject(deviceUpdated)}]");


            return CreatedAtRoute("getDevice", new {id = deviceUpdated.Id}, deviceUpdated);
        }

        /// <inheritdoc />
        /// <summary>
        /// Delete the specified device by id.
        /// </summary>
        /// <returns>null</returns>
        /// <param name="id">Device Identifier.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="404">Device not found in database.</response>
        /// <response code="423">Device is locked.</response>
        /// <response code="500">Internal failure.</response>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            if (device.Status == DeviceStatus.Locked || device.Status == DeviceStatus.LockedOffline)
            {
                return StatusCodeExtension(423, "Device is locked.");
            }

            try
            {
                _devicesRepository.Remove(id);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to Remove device from database. " + ex.Message);
            }

            return OkExtension(string.Format("Successfully deleted device: [{0}]", device));
        }

        /// <summary>
        /// Restarts the device.
        /// </summary>
        /// <returns>null</returns>
        /// <param name="id">device id</param>
        [HttpPost("{id}/restart")]
        public IActionResult RestartDevice(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);
            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            if (!device.Available)
            {
                return StatusCodeExtension(423, "Device is locked.");
            }

            var restartOutput = _deviceUtils.RestartDevice(device);

            if (!string.IsNullOrEmpty(restartOutput))
            {
                return StatusCodeExtension(500, $"Failed to restart device. [{restartOutput}]");
            }

            return OkExtension("RestartDevice successful.");
        }

        /// <summary>
        /// Contains ids of devices currently downloading screenshots from devices.
        /// </summary>
        public readonly List<string> ScreenshotLocked = new List<string>();

        /// <summary>
        /// Time to wait for screenshotLock to get free - in ms.
        /// </summary>
        public int ScreenshotLockedTimeout = 20000;

        /// <summary>
        /// Gets device screenshot the by identifier.
        /// </summary>
        /// <returns>The device screenshot.</returns>
        /// <param name="id">Identifier.</param>
        /// <response code="200">Device returned successfully.</response>
        /// <response code="404">Device not found.</response>
        [HttpGet("{id}/screenshot", Name = "getDeviceScreenshot")]
        public IActionResult GetDeviceScreenshotById(string id)
        {
            LogRequestToDebug();

            var device = _devicesRepository.Find(id);

            if (device == null)
            {
                return NotFoundExtension("Device not found in database.");
            }

            if (device.Status == DeviceStatus.Offline || device.Status == DeviceStatus.FailedToInitialize ||
                device.Status == DeviceStatus.LockedOffline)
            {
                try
                {
                    return _screenshotService.LoadScreenshotForOfflineDevice(device);
                }
                catch (Exception e)
                {
                    return StatusCodeExtension(500, e.Message);
                }
            }

            var start = DateTime.Now;
            while (ScreenshotLocked.Contains(device.Id))
            {
                if (start + TimeSpan.FromMilliseconds(ScreenshotLockedTimeout) <= DateTime.Now)
                {
                    throw new TimeoutException($"Too many requests for screenshots on device {device.Id}.");
                }

                Thread.Sleep(100);
            }

            ScreenshotLocked.Add(device.Id);

            try
            {
                switch (device.Type)
                {
                    case DeviceType.IOS:
                        return _screenshotService.TakeScreenshotIosDevice(device);
                    case DeviceType.Android:
                        return _screenshotService.TakeScreenshotAndroidDevice(device);
                    case DeviceType.Unspecified:
                        return NotFoundExtension($"{device.Type} devices are not supported for screenshots.");
                    default:
                        return NotFoundExtension($"{device.Type} devices are not supported for screenshots.");
                }
            }
            catch (Exception e)
            {
                return StatusCodeExtension(500, e.Message);
            }
            finally
            {
                ScreenshotLocked.Remove(device.Id);
            }
        }
    }
}
