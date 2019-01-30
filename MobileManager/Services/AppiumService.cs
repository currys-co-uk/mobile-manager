using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MobileManager.Appium;
using MobileManager.Configuration.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices.Enums;
using MobileManager.Services.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Appium service.
    /// </summary>
    public class AppiumService : IAppiumService
    {
        private readonly IManagerLogger _logger;
        private readonly RestClient _restClient;

        private readonly string _appiumLogFolderPath;
        private readonly string _ideviceSyslogFolderPath;
        private readonly IExternalProcesses _externalProcesses;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Services.AppiumService"/> class.
        /// </summary>
        public AppiumService(IManagerConfiguration configuration, IManagerLogger logger, IExternalProcesses externalProcesses)
        {
            _logger = logger;
            _externalProcesses = externalProcesses;
            _restClient = new RestClient(configuration, _logger);

            _appiumLogFolderPath = EnsureLogFolderIsCreated(configuration.AppiumLogFilePath);
            _ideviceSyslogFolderPath = EnsureLogFolderIsCreated(configuration.IdeviceSyslogFolderPath);
        }

        private string EnsureLogFolderIsCreated(string configFolderPath)
        {
            var path = configFolderPath.StartsWith("~")
                ? configFolderPath.Replace("~",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                : configFolderPath;

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to create AppiumLogFilePath [{configFolderPath}].", e);
            }

            return path;
        }

        /// <inheritdoc />
        /// <summary>
        /// Starts the appium for device identifier.
        /// </summary>
        /// <returns>The appium for device identifier.</returns>
        /// <param name="deviceId">Device identifier.</param>
        public async Task<string> StartAppiumForDeviceId(string deviceId)
        {
            _logger.Info($"{nameof(StartAppiumForDeviceId)} {deviceId} Thread Started.");
            var configuration = await _restClient.GetManagerConfiguration();
            var device = await _restClient.GetDevice(deviceId);

            _logger.Debug($"{nameof(StartAppiumForDeviceId)} - device: {JsonConvert.SerializeObject(device)}");

            var appiumIpAddress = configuration.LocalIpAddress;
            var currentAppiumProcesses = await _restClient.GetAppiumProcesses();

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - currentAppiumProcesses: {JsonConvert.SerializeObject(currentAppiumProcesses)}");

            var usedAppiumPorts = new List<string>();
            foreach (var process in currentAppiumProcesses)
            {
                usedAppiumPorts.Add(process.AppiumPort);
                usedAppiumPorts.Add(process.AppiumBootstrapPort);
                usedAppiumPorts.Add(process.WebkitDebugProxyPort);
                usedAppiumPorts.Add(process.WdaLocalPort);
            }

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - usedAppiumPorts: {JsonConvert.SerializeObject(usedAppiumPorts)}");

            var appiumPort = GetFreePortAsyncMac(configuration, usedAppiumPorts);
            usedAppiumPorts.Add(appiumPort);
            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - new appiumPort: {JsonConvert.SerializeObject(appiumPort)}");

            var appiumBootstrapPort = GetFreePortAsyncMac(configuration, usedAppiumPorts);
            usedAppiumPorts.Add(appiumBootstrapPort);
            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - new appiumBootstrapPort: {JsonConvert.SerializeObject(appiumBootstrapPort)}");

            var appiumLogFilePath = _appiumLogFolderPath;
            if (!appiumLogFilePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                appiumLogFilePath += Path.DirectorySeparatorChar;
            }

            appiumLogFilePath = appiumLogFilePath + deviceId + ".log";
            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - appiumLogFilePath: {JsonConvert.SerializeObject(appiumLogFilePath)}");

            var webkitDebugProxyPort = GetFreePortAsyncMac(configuration, usedAppiumPorts);
            usedAppiumPorts.Add(webkitDebugProxyPort);

            var wdaLocalPort = GetFreePortAsyncMac(configuration, usedAppiumPorts);
            usedAppiumPorts.Add(wdaLocalPort);
            /*
            String defaultCapabilities = "\\\"automation-name\\\" : \\\"XCUITest\\\"," +
                " \\\"teamId\\\" : \\\"CB52FCDD4H\\\"," +
                " \\\"signingId\\\" : \\\"iPhone Developer\\\"," +
                " \\\"showXcodeLog\\\" : \\\"true\\\"," +
                " \\\"realDeviceLogger\\\" : \\\"/usr/local/lib/node_modules/deviceconsole/deviceconsole\\\"," +
                " \\\"bootstrapPath\\\" : \\\"/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent\\\"," +
                " \\\"agentPath\\\" : \\\"/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent/WebDriverAgent.xcodeproj\\\"," +
                " \\\"startIWDP\\\" : \\\"true\\\"," +
                " \\\"sessionTimeout\\\" : \\\"6000\\\"";
            */
            var appiumArguments = "" +
                                  " --session-override" +
                                  " --suppress-adb-kill-server" +
                                  " --log " + appiumLogFilePath +
                                  " --udid " + deviceId +
                                  " --address " + appiumIpAddress +
                                  " --port " + appiumPort +
                                  " --bootstrap-port " + appiumBootstrapPort +
                                  " --webkit-debug-proxy-port " + webkitDebugProxyPort +
                                  " --webdriveragent-port " + wdaLocalPort;

            /*
            if (device.Type == Devices.Enums.DeviceType.iOS)
            {
                appiumArguments += " --default-capabilities \\\'{"+defaultCapabilities+"}\\\'";
            }
            */

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - starting appium with args: {JsonConvert.SerializeObject(appiumArguments)}");

            var appiumPid = _externalProcesses.RunProcessInBackground("appium", appiumArguments);

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - appium started in terminal with pid: {JsonConvert.SerializeObject(appiumPid)}");

            Thread.Sleep(3000);
            var psOutput = _externalProcesses.RunProcessAndReadOutput("ps", "ax");

            var runningAppiumProcess = psOutput.Split('\n').Where(x => x.Contains("bin/appium") && x.Contains(deviceId))
                .ToList();

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - runningAppiumProcess {JsonConvert.SerializeObject(runningAppiumProcess)} for device {device}.");

            var processId = "";
            if (runningAppiumProcess.Count() != 1)
            {
                _logger.Error(
                    $"{nameof(StartAppiumForDeviceId)}: Multiple appium processes running with deviceId={deviceId}. Killing them all...");
                _externalProcesses.StopProcessRunningInBackground(deviceId);
            }
            else
            {
                processId = runningAppiumProcess.First().Trim().Split()[0];
                _logger.Debug(
                    $"{nameof(StartAppiumForDeviceId)} - runningAppiumProcess with pid: {JsonConvert.SerializeObject(processId)}.");
            }

            if (string.IsNullOrEmpty(processId) &&
                !_externalProcesses.IsProcessInBackgroundRunning(Convert.ToInt32(processId)))
            {
                throw new Exception($"{nameof(StartAppiumForDeviceId)}: Appium process failed to start successfully.");
            }

            appiumPid = Convert.ToInt32(processId);

            var appiumProcess = new AppiumProcess(deviceId, appiumPort, appiumBootstrapPort, appiumPid,
                webkitDebugProxyPort, wdaLocalPort);

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - adding appiumProcess: {JsonConvert.SerializeObject(appiumProcess)}.");

            var result = Task.Run(async () => await _restClient.AddAppiumProcess(appiumProcess)).Result;

            _logger.Debug(
                $"{nameof(StartAppiumForDeviceId)} - finished adding appiumProcess: with result [{result}].");

            if (device.Type == DeviceType.IOS)
            {
                _logger.Debug($"{nameof(StartAppiumForDeviceId)} - starting iosWebkit for ios device {device}.");

                var iosWebkitPid = StartIosWebkitDebugProxy(deviceId, webkitDebugProxyPort);
                if (!_externalProcesses.IsProcessInBackgroundRunning(iosWebkitPid))
                {
                    _logger.Error(
                        $"{nameof(StartAppiumForDeviceId)} - ios_webkit_debug_proxy process failed to start successfully.");

                    throw new Exception(
                        $"{nameof(StartAppiumForDeviceId)}: ios_webkit_debug_proxy process failed to start successfully.");
                }

                _logger.Debug($"{nameof(StartAppiumForDeviceId)} - starting IdeviceSyslog for ios device {device}.");

                StartIdeviceSyslog(deviceId);
            }

            _logger.Info($"{nameof(StartAppiumForDeviceId)} appiumEndpoint: {appiumIpAddress}:{appiumPort}");
            var appiumEndpoint = new Uri($"http://{appiumIpAddress}:{appiumPort}/wd/hub");

            return appiumEndpoint.ToString();
        }

        /// <inheritdoc />
        /// <summary>
        /// Stops the appium for device identifier async.
        /// </summary>
        /// <returns>The appium for device identifier async.</returns>
        /// <param name="deviceId">Device identifier.</param>
        public async Task<bool> StopAppiumForDeviceIdAsync(string deviceId)
        {
            _externalProcesses.StopProcessRunningInBackground(deviceId);

            var psOutput = _externalProcesses.RunProcessAndReadOutput("ps", "ax");
            var runningIProxyForDevice = psOutput.Split('\n').Where(x => x.Contains("iproxy") && x.Contains(deviceId));

            _logger.Debug(
                $"{nameof(StopAppiumForDeviceIdAsync)} - runningIProxyForDevice {JsonConvert.SerializeObject(runningIProxyForDevice)} for device {deviceId}.");
            foreach (var process in runningIProxyForDevice)
            {
                _externalProcesses.RunProcessAndReadOutput("kill", process);
            }

            return await _restClient.RemoveAppiumProcess(deviceId);
        }

        private int StartIosWebkitDebugProxy(string deviceId, string webkitPort)
        {
            var proxyArguments = $"-c {deviceId}:{webkitPort}";
            return _externalProcesses.RunProcessInBackground("ios_webkit_debug_proxy", proxyArguments);
        }

        private int StartIdeviceSyslog(string deviceId)
        {
            var syslogArguments = $"-u {deviceId} 2>&1 > {_ideviceSyslogFolderPath}/{deviceId}.log";
            return _externalProcesses.RunProcessInBackground("idevicesyslog", syslogArguments);
        }

        private string GetFreePortAsync(IManagerConfiguration configuration, string ignorePort)
        {
            var portStartIndex = configuration.AppiumPortRangeMin;
            var portEndIndex = configuration.AppiumPortRangeMax;

            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();

            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();
            var unusedPort = 0;

            _logger.Debug("usedPorts: " + JsonConvert.SerializeObject(usedPorts));

            for (var port = portStartIndex; port < portEndIndex; port++)
            {
                if (!usedPorts.Contains(port) && !ignorePort.Equals(port.ToString()))
                {
                    unusedPort = port;
                    break;
                }
            }

            return unusedPort.ToString();
        }

        private string GetFreePortAsyncMac(IManagerConfiguration configuration, ICollection<string> ignorePort)
        {
            var portStartIndex = configuration.AppiumPortRangeMin;
            var portEndIndex = configuration.AppiumPortRangeMax;

            for (var port = portStartIndex; port < portEndIndex; port++)
            {
                if (IsPortAvailable(port) && !ignorePort.Contains(port.ToString()))
                {
                    return port.ToString();
                }
            }

            return "";
        }

        private bool IsPortAvailable(int port)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "lsof",
                Arguments = string.Format($"-nP -iTCP:{port} -sTCP:LISTEN"),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(processInfo))
            {
                using (var reader = process?.StandardOutput)
                {
                    var stdout = reader?.ReadToEnd();

                    if (stdout != null && stdout.Contains(":" + port))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
