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
using MobileManager.Utils;
using Newtonsoft.Json;

namespace MobileManager.Services
{
    /// <inheritdoc cref="IHostedService" />
    /// <summary>
    /// Android device service.
    /// </summary>
    public class AndroidDeviceService : IHostedService, IDisposable
    {
        private readonly IManagerLogger _logger;
        private readonly RestClient _restClient;

        private Task _androidDeviceService;
        private readonly DeviceUtils _deviceUtils;
        private IExternalProcesses _externalProcesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Services.AndroidDeviceService"/> class.
        /// </summary>
        public AndroidDeviceService(IManagerConfiguration configuration, IManagerLogger logger, IExternalProcesses externalProcesses)
        {
            _logger = logger;
            _externalProcesses = externalProcesses;
            _logger.Debug("Running AndroidDeviceService service.");
            _deviceUtils = new DeviceUtils(_logger, _externalProcesses);
            _restClient = new RestClient(configuration, _logger);
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _androidDeviceService =
                Task.Factory.StartNew(async () => { await LoadConnectedAndroidDevicesAsync(cancellationToken); },
                    cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _externalProcesses.RunProcessAndReadOutput("adb", "kill-server");
            _androidDeviceService.Wait(cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _androidDeviceService?.Dispose();
        }


        private async Task LoadConnectedAndroidDevicesAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(LoadConnectedAndroidDevicesAsync)} Thread started.");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.Debug($"Running {nameof(LoadConnectedAndroidDevicesAsync)}.");

                    if (await _restClient.TryToConnect())
                    {
                        var deviceIdAndStatus = GetAndroidDevicesFromAdbDevicesOutput();

                        await _deviceUtils.CheckAllDevicesInDevicePoolAreOnline(deviceIdAndStatus.Keys.ToList(),
                            DeviceType.Android, _restClient);

                        foreach (var deviceId in deviceIdAndStatus.Keys)
                        {
                            var deviceAlreadyInPool = await IsDeviceAlreadyInDevicePoolAsync(deviceId);
                            if (!deviceAlreadyInPool)
                            {
                                var state = deviceIdAndStatus[deviceId];
                                if (state != "device")
                                {
                                    _logger.Error(
                                        $"{nameof(LoadConnectedAndroidDevicesAsync)}: Device with id: [{deviceId}] is in incorrect state: [{state}]. Expected state is [device]");
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            _logger.Debug(
                                $"{nameof(LoadConnectedAndroidDevicesAsync)}: read device [{deviceId}] properties.");
                            var deviceName = GetDeviceName(deviceId);

                            if (string.IsNullOrWhiteSpace(deviceName))
                            {
                                _logger.Error(
                                    $"{nameof(LoadConnectedAndroidDevicesAsync)}: Failed to get deviceName to device with id: '" +
                                    deviceId + "'");
                                continue;
                            }

                            _logger.Debug(
                                $"{nameof(LoadConnectedAndroidDevicesAsync)}: new android device factory [{deviceId}] name [{deviceName.Trim('\n', '\r')}].");
                            var device = new DeviceFactory().NewDevice(deviceId, deviceName.Trim('\n', '\r'), true,
                                DeviceType.Android, DeviceStatus.Online);

                            var deviceProperties = GetDevicePropertiesById(deviceId);

                            var properties = new List<DeviceProperties>();
                            foreach (var prop in deviceProperties)
                            {
                                properties.Add(new DeviceProperties(prop.Key, prop.Value));
                            }

                            device.Properties = properties;

                            await TryAddNewDeviceToDevicePoolAsync(device);

                            _logger.Debug(
                                $"{nameof(LoadConnectedAndroidDevicesAsync)}: TryAddNewDeviceToDevicePoolAsync [{JsonConvert.SerializeObject(device)}].");
                            await TryAddNewDeviceToDevicePoolAsync(device);
                        }

                        var sleepTime = (await _restClient.GetManagerConfiguration()).AndroidDeviceServiceRefreshTime;
                        _logger.Debug($"{nameof(LoadConnectedAndroidDevicesAsync)}: sleep for [{sleepTime}].");
                        Thread.Sleep(sleepTime);
                    }
                    else
                    {
                        _logger.Error($"{nameof(LoadConnectedAndroidDevicesAsync)}: Failed connecting to " +
                                      _restClient.Endpoint +
                                      " [STOP]");
                        var sleep = AppConfigurationProvider.Get<ManagerConfiguration>().GlobalReconnectTimeout;
                        _logger.Info($"{nameof(LoadConnectedAndroidDevicesAsync)}: Sleep for [ms]: {sleep}");
                        Thread.Sleep(sleep);
                        _logger.Info($"{nameof(LoadConnectedAndroidDevicesAsync)}: Sleep finished");
                    }
                }

                _logger.Info($"{nameof(LoadConnectedAndroidDevicesAsync)} STOP.");
            }
            catch (Exception e)
            {
                _logger.Error($"Stopping {nameof(LoadConnectedAndroidDevicesAsync)}.", e);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                var result = _externalProcesses.RunProcessAndReadOutput("adb", "kill-server");
                _logger.Debug(
                    $"{nameof(LoadConnectedAndroidDevicesAsync)}: Stop ADB server to release ports - output:{result}");
            }
        }

        private string GetDeviceName(string deviceId)
        {
            var deviceModelName =
                _externalProcesses.RunProcessAndReadOutput("adb",
                    $"-s {deviceId} shell settings get global device_name");

            var deviceName = $"{deviceModelName.Trim('\n', '\r')}";

            if (deviceName == "null")
            {
                deviceModelName =
                    _externalProcesses.RunProcessAndReadOutput("adb", "-s " + deviceId + " shell getprop net.hostname");
                deviceName = $"{deviceModelName.Trim('\n', '\r')}";
            }

            if (deviceName.Contains("Error"))
            {
                _logger.Error($"Failed to get correct device name [{deviceName}]");
                return null;
            }

            return deviceName;
        }

        private async Task TryAddNewDeviceToDevicePoolAsync(Device device)
        {
            try
            {
                var addedDevice = await _restClient.AddDevice(device);
                _logger.Info("Added new device: " + addedDevice.Id);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error("Failed Added new device: " + device.Id + ". " + ex.Message);
            }
        }

        private Dictionary<string, string> GetDevicePropertiesById(string deviceId)
        {
            var properties = new Dictionary<string, string>();

            var devicePropertiesOutput =
                _externalProcesses.RunProcessAndReadOutput("adb", $"-s {deviceId} shell getprop");

            using (var reader = new StringReader(devicePropertiesOutput))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var splitBy = new[] {':'};
                    var key = line.Split(splitBy, 2)[0].Trim().Trim('[', ']');
                    var val = line.Split(splitBy, 2)[1].Trim().Trim('[', ']');

                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        properties.Add(key, val);
                    }
                }
            }

            return properties;
        }

        private Dictionary<string, string> GetAndroidDevicesFromAdbDevicesOutput()
        {
            var output = _externalProcesses.RunProcessAndReadOutput("adb", "devices");
            _logger.Debug($"{nameof(GetAndroidDevicesFromAdbDevicesOutput)}: adb output [{output}]");

            var listOfAndroidDeviceIdsAndStatus =
                output.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();
            _logger.Debug(
                $"{nameof(GetAndroidDevicesFromAdbDevicesOutput)}: listOfAndroidDeviceIdsAndStatus [{JsonConvert.SerializeObject(listOfAndroidDeviceIdsAndStatus)}]");

            listOfAndroidDeviceIdsAndStatus = listOfAndroidDeviceIdsAndStatus.Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList();
            _logger.Debug(
                $"{nameof(GetAndroidDevicesFromAdbDevicesOutput)}: cleaning listOfAndroidDeviceIdsAndStatus [{JsonConvert.SerializeObject(listOfAndroidDeviceIdsAndStatus)}]");

            var deviceIdsAndStatus = new Dictionary<string, string>();
            foreach (var item in listOfAndroidDeviceIdsAndStatus)
            {
                var line = item.Split('\t');
                if (line.Length != 2)
                {
                    continue;
                }

                var id = line[0];
                var status = line[1];
                deviceIdsAndStatus.Add(id, status);
                _logger.Debug(
                    $"GetAndroidDevicesFromAdbDevicesOutput: adding new device with id[{id}] and status [{status}]");
            }

            return deviceIdsAndStatus;
        }

        private async Task<bool> IsDeviceAlreadyInDevicePoolAsync(string deviceId)
        {
            var deviceInDevicePool = await _restClient.GetDevice(deviceId);
            _logger.Debug(
                $"{nameof(IsDeviceAlreadyInDevicePoolAsync)}: devices in pool [{JsonConvert.SerializeObject(deviceInDevicePool)}]");

            if (deviceInDevicePool != null)
            {
                _logger.Debug(
                    $"{nameof(IsDeviceAlreadyInDevicePoolAsync)}: devices [{deviceInDevicePool}], status [{deviceInDevicePool.Status}]");

                if (deviceInDevicePool.Status == DeviceStatus.Offline)
                {
                    deviceInDevicePool.Status = DeviceStatus.Online;
                    deviceInDevicePool.Available = true;
                    await _restClient.UpdateDevice(deviceInDevicePool);
                    _logger.Debug(
                        $"{nameof(IsDeviceAlreadyInDevicePoolAsync)}: update device [{JsonConvert.SerializeObject(deviceInDevicePool)}]");

                    return true;
                }

                _logger.Info($"{nameof(IsDeviceAlreadyInDevicePoolAsync)}: Device " + deviceId +
                             " is already stored in database.");
                return true;
            }

            return false;
        }
    }
}
