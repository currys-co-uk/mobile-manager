using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MobileManager.Configuration.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Services;
using MobileManager.Services.Interfaces;
using MobileManager.Utils;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace MobileManagerTests
{
    public class ReservationServiceTests
    {
        private readonly Device _device1 =
            new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

        private readonly Device _device2 =
            new DeviceFactory().NewDevice("222", "device_222", true, DeviceType.Android, DeviceStatus.Online);

        private readonly IExternalProcesses _externalProcesses = new Mock<IExternalProcesses>().Object;

        private static readonly IManagerLogger Logger = new Mock<IManagerLogger>().Object;
        private readonly string _httpLocalhost;
        private readonly Mock<IManagerConfiguration> _config;

        public ReservationServiceTests()
        {
            _config = new Mock<IManagerConfiguration>();
            _config.Setup(c => c.LocalIpAddress).Returns("localhost");
            _config.Setup(c => c.ListeningPort).Returns(9876);
            _config.Setup(c => c.AppiumLogFilePath).Returns("appiumLogs");
            _config.Setup(c => c.IdeviceSyslogFolderPath).Returns("syslogs");

            _httpLocalhost =
                $"http://{_config.Object.LocalIpAddress}:{_config.Object.ListeningPort}";
        }

        [Fact]
        public async void LockSingleDeviceAsync()
        {
            // Arrange
            var lockedDevice = _device1;
            lockedDevice.Available = true;
            lockedDevice.AppiumEndpoint = "http://localhost:1234";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, $"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(lockedDevice));
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            var mockAppiumService = new Mock<IAppiumService>();
            mockAppiumService.Setup(appiumService => appiumService.StartAppiumForDeviceId(_device1.Id))
                .Returns(Task.FromResult(lockedDevice.AppiumEndpoint));

            // Act
            var deviceUtils = new DeviceUtils(Logger, _externalProcesses);
            var restClient = new RestClient(_config.Object, new HttpClient(mockHttp), Logger);
            var result = await deviceUtils.LockDevice(_device1.Id, restClient, mockAppiumService.Object);

            // Assert
            Assert.IsType<Device>(result);
            Assert.Equal(lockedDevice.Id, result.Id);
            Assert.Equal(lockedDevice.Available, result.Available);
            Assert.Equal(lockedDevice.AppiumEndpoint, result.AppiumEndpoint);
        }

        [Fact]
        public async void UnlockSingleDeviceAsync()
        {
            // Arrange
            var unlockedDevice = _device1;
            unlockedDevice.Available = true;
            unlockedDevice.AppiumEndpoint = "";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, $"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(unlockedDevice));
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            var mockAppiumService = new Mock<IAppiumService>();
            mockAppiumService.Setup(appiumService => appiumService.StopAppiumForDeviceIdAsync(_device1.Id))
                .Returns(Task.FromResult(true));

            // Act
            var deviceUtils = new DeviceUtils(Logger, _externalProcesses);
            var restClient = new RestClient(_config.Object, new HttpClient(mockHttp), Logger);
            var result = await deviceUtils.UnlockDevice(_device1.Id, restClient, mockAppiumService.Object);

            // Assert
            Assert.IsType<Device>(result);
            Assert.Equal(unlockedDevice.Id, result.Id);
            Assert.Equal(unlockedDevice.Available, result.Available);
            Assert.Equal(unlockedDevice.AppiumEndpoint, result.AppiumEndpoint);
        }

        [Fact]
        public async void FindMatchingDevice_byIdAsync()
        {
            // Arrange
            var requestedDevice = new RequestedDevices
            {
                DeviceId = _device1.Id
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            // Act
            var deviceUtils = new DeviceUtils(Logger, _externalProcesses);
            var restClient = new RestClient(_config.Object, new HttpClient(mockHttp), Logger);
            var result = await deviceUtils.FindMatchingDevice(requestedDevice, restClient);

            // Assert
            var viewResult = Assert.IsType<Device>(result);
            Assert.Equal(_device1.Id, viewResult.Id);
            Assert.True(viewResult.Available);
        }

        [Fact]
        public async void FindMatchingDevice_byDeviceType_iOSAsync()
        {
            // Arrange
            var requestedDevice = new RequestedDevices
            {
                DeviceType = DeviceType.IOS
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1, _device2}));

            // Act
            var deviceUtils = new DeviceUtils(Logger, _externalProcesses);
            var restClient = new RestClient(_config.Object, new HttpClient(mockHttp), Logger);
            var result = await deviceUtils.FindMatchingDevice(requestedDevice, restClient);

            // Assert
            var viewResult = Assert.IsType<Device>(result);
            Assert.Equal(DeviceType.IOS, viewResult.Type);
            Assert.True(viewResult.Available);
        }

        [Fact]
        public async void FindMatchingDevice_byDeviceType_AndroidAsync()
        {
            // Arrange
            var requestedDevice = new RequestedDevices
            {
                DeviceType = DeviceType.Android
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1, _device2}));

            // Act
            var deviceUtils = new DeviceUtils(Logger, _externalProcesses);
            var restClient = new RestClient(_config.Object, new HttpClient(mockHttp), Logger);
            var result = await deviceUtils.FindMatchingDevice(requestedDevice, restClient);

            // Assert
            var viewResult = Assert.IsType<Device>(result);
            Assert.Equal(DeviceType.Android, viewResult.Type);
            Assert.True(viewResult.Available);
        }
    }
}
