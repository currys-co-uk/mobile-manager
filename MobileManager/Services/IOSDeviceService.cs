using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Devices.Interfaces;
using MobileManager.Utils;

namespace MobileManager.Services
{
    /// <inheritdoc cref="IHostedService" />
    /// <summary>
    /// IOS Device connector.
    /// </summary>
    public class IosDeviceService : IHostedService, IDisposable
    {
        private readonly IManagerLogger _logger;
        private readonly RestClient _restClient;
        private Task _iosDeviceService;
        private readonly DeviceUtils _deviceUtils;
        private readonly IExternalProcesses _externalProcesses;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:DeviceConnectors.iOS.IOSDeviceConnector"/> class.
        /// </summary>
        public IosDeviceService(IManagerConfiguration configuration, IManagerLogger logger,
            IExternalProcesses externalProcesses)
        {
            _logger = logger;
            _externalProcesses = externalProcesses;
            _deviceUtils = new DeviceUtils(_logger, _externalProcesses);
            _logger.Info("Running IOSDeviceConnector service.");
            _restClient = new RestClient(configuration, _logger);
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _iosDeviceService =
                Task.Factory.StartNew(async () => { await LoadConnectedIosDevicesAsync(cancellationToken); },
                    cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _iosDeviceService.Wait(cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _iosDeviceService?.Dispose();
        }

        /// <summary>
        /// Loads the connected IOS Devices async.
        /// </summary>
        /// <returns>The connected IOS Devices async.</returns>
        private async Task LoadConnectedIosDevicesAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(LoadConnectedIosDevicesAsync)} Thread started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.Debug($"Running {nameof(LoadConnectedIosDevicesAsync)}.");

                    if (await _restClient.TryToConnect())
                    {
                        var output = _externalProcesses.RunProcessAndReadOutput("idevice_id", "-l");
                        var listOfIosDeviceIds =
                            output.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();
                        listOfIosDeviceIds = listOfIosDeviceIds.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct()
                            .ToList();

                        await _deviceUtils.CheckAllDevicesInDevicePoolAreOnline(listOfIosDeviceIds, DeviceType.IOS,
                            _restClient);

                        foreach (var deviceId in listOfIosDeviceIds)
                        {
                            if (string.IsNullOrEmpty(deviceId))
                            {
                                continue;
                            }

                            var deviceInDevicePool = await _restClient.GetDevice(deviceId);
                            if (deviceInDevicePool != null)
                            {
                                if (deviceInDevicePool.Status == DeviceStatus.Offline)
                                {
                                    SetNewDeviceProperties(deviceInDevicePool);
                                    await MountDeveloperDiskAsync(deviceInDevicePool);
                                    deviceInDevicePool.Status = DeviceStatus.Online;
                                    deviceInDevicePool.Available = true;
                                    await _restClient.UpdateDevice(deviceInDevicePool);
                                    continue;
                                }

                                _logger.Info(
                                    $"{nameof(LoadConnectedIosDevicesAsync)}: Device {deviceId} is already stored in database.");
                                await MountDeveloperDiskAsync(deviceInDevicePool);
                                continue;
                            }

                            var deviceName =
                                _externalProcesses.RunProcessAndReadOutput("idevicename", $"-u {deviceId}");

                            if (string.IsNullOrEmpty(deviceName) || deviceName.Contains("ERROR"))
                            {
                                _logger.Error(
                                    $"{nameof(LoadConnectedIosDevicesAsync)}: Failed get device name for deviceId: [{deviceId}].\n[{deviceName}]");
                                continue;
                            }

                            var device = new DeviceFactory().NewDevice(deviceId, deviceName.Trim('\n'), true,
                                DeviceType.IOS, DeviceStatus.Online);
                            SetNewDeviceProperties(device);

                            await TryAddNewDeviceToDevicePoolAsync(device);

                            await MountDeveloperDiskAsync(device);
                        }

                        Thread.Sleep((await _restClient.GetManagerConfiguration()).IosDeviceServiceRefreshTime);
                    }
                    else
                    {
                        _logger.Error(
                            $"{nameof(LoadConnectedIosDevicesAsync)}: Failed connecting to {_restClient.Endpoint} [STOP]");
                        WaitForGlobalReconnectTimeout();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"{nameof(LoadConnectedIosDevicesAsync)} exception.", e);
                    WaitForGlobalReconnectTimeout();
                }
            }
        }

        private void WaitForGlobalReconnectTimeout()
        {
            var sleep = AppConfigurationProvider.Get<ManagerConfiguration>().GlobalReconnectTimeout;
            _logger.Info($"{nameof(WaitForGlobalReconnectTimeout)} Sleep for [ms]: {sleep}");
            Thread.Sleep(sleep);
            _logger.Info($"{nameof(WaitForGlobalReconnectTimeout)} Sleep finished");
        }

        private void SetNewDeviceProperties(IDevice device)
        {
            var deviceProperties = GetDevicePropertiesById(device.Id);

            var properties = new List<DeviceProperties>();
            foreach (var prop in deviceProperties)
            {
                properties.Add(new DeviceProperties(prop.Key, prop.Value));
            }

            device.Properties = properties;
        }

        private async Task TryAddNewDeviceToDevicePoolAsync(Device device)
        {
            try
            {
                var addedDevice = await _restClient.AddDevice(device);
                _logger.Info($"{nameof(TryAddNewDeviceToDevicePoolAsync)}: Added new device: {addedDevice.Id}");
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(
                    $"{nameof(TryAddNewDeviceToDevicePoolAsync)}: Failed Added new device: {device.Id}. {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the device properties by identifier.
        /// </summary>
        /// <returns>The device properties by identifier.</returns>
        /// <param name="deviceId">Device identifier.</param>
        private Dictionary<string, string> GetDevicePropertiesById(string deviceId)
        {
            var properties = new Dictionary<string, string>();

            var devicePropertiesOutput = _externalProcesses.RunProcessAndReadOutput("ideviceinfo", "-u " + deviceId);

            using (var reader = new StringReader(devicePropertiesOutput))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var splitBy = new[] {':'};
                    var key = line.Split(splitBy, 2)[0];
                    if (key.StartsWith(" ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var val = line.Split(splitBy, 2)[1];

                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        properties.Add(key.Trim(), val.Trim());
                    }
                }
            }

            return properties;
        }

        private async Task MountDeveloperDiskAsync(IDevice device)
        {
            _logger.Debug($"{nameof(MountDeveloperDiskAsync)}: device [{device.Id}]");

            device.Status = DeviceStatus.Initialize;
            var xcodePath = (await _restClient.GetManagerConfiguration()).XcodePath;

            if (string.IsNullOrEmpty(xcodePath))
            {
                throw new Exception($"{nameof(MountDeveloperDiskAsync)}: Path to Xcode is not set in configuration!");
            }

            var productVersion = device.Properties.First(a => a.Key == "ProductVersion").Value.Trim();

            var majorProductVersion = productVersion.Split('.')[0];
            var minorProductVersion = productVersion.Split('.')[1];

            var editedProductVersion = $"{majorProductVersion}.{minorProductVersion}";
            _logger.Debug($"{nameof(MountDeveloperDiskAsync)}: using productVersion [{editedProductVersion}]");

            var developerDiskImagePath = GetDeveloperDiskImagePath(xcodePath, editedProductVersion);

            if (!File.Exists(developerDiskImagePath))
            {
                device.Status = DeviceStatus.FailedToInitialize;
                throw new FileNotFoundException("Path to Developer image for version:" + productVersion +
                                                " does not exist. path=" + developerDiskImagePath +
                                                ". Consider updating Xcode.");
            }

            var imageSignature = _externalProcesses.RunProcessAndReadOutput("ideviceimagemounter",
                $" -u {device.Id} -l", 10000);

            _logger.Debug($"{nameof(MountDeveloperDiskAsync)}: {nameof(imageSignature)} [{imageSignature}]");

            if (imageSignature.Contains("ImageSignature["))
            {
                _logger.Debug($"Developer disk already mounted: {imageSignature}");
                device.Status = DeviceStatus.Online;
                return;
            }

            var ret = _externalProcesses.RunProcessAndReadOutput("ideviceimagemounter",
                $" -u {device.Id} -t Developer \"{developerDiskImagePath}\"",
                10000);

            _logger.Debug($"{nameof(MountDeveloperDiskAsync)}: {nameof(developerDiskImagePath)} [{ret}]");


            if (ret.Contains("Error: mount_image returned -3"))
            {
                device.Status = DeviceStatus.FailedToInitialize;
                throw new Exception(
                    $"{nameof(MountDeveloperDiskAsync)}: Failed to mount developer image to device. Device has to be unlocked!");
            }

            if (ret.Contains("Error:"))
            {
                device.Status = DeviceStatus.FailedToInitialize;
                throw new Exception(
                    $"{nameof(MountDeveloperDiskAsync)}: Failed to mount developer image to device. {ret}");
            }

            if (ret.Contains("No device found, is it plugged in?"))
            {
                device.Status = DeviceStatus.FailedToInitialize;
                throw new Exception(
                    $"{nameof(MountDeveloperDiskAsync)}: Failed to mount developer image to device. {ret}");
            }

            _logger.Debug($"{nameof(MountDeveloperDiskAsync)}: Mounting Developer disk returned: {ret}");
        }

        private static string GetDeveloperDiskImagePath(string xcodePath, string editedProductVersion)
        {
            var developerDiskImageFolderPath =
                xcodePath + "/Contents/Developer/Platforms/iPhoneOS.platform/DeviceSupport/";
            var developerDiskVersionFolders = Directory.EnumerateDirectories(developerDiskImageFolderPath);
            var appropriateVersionFolder =
                developerDiskVersionFolders.FirstOrDefault(x => x.Contains(editedProductVersion));

            if (appropriateVersionFolder == null)
            {
                throw new FileNotFoundException(Path.Combine(developerDiskImageFolderPath, editedProductVersion));
            }

            return Path.Combine(appropriateVersionFolder, "DeveloperDiskImage.dmg");
        }
    }
}
