using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Adb;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Services;
using Moq;
using Xunit;

namespace MobileManagerTests.Controllers
{
    public class AdbControllerTests
    {
        private readonly Mock<IManagerConfiguration> _config;

        private readonly IExternalProcesses _externalProcesses = new Mock<IExternalProcesses>().Object;

        private static readonly IManagerLogger Logger = new Mock<IManagerLogger>().Object;

        public AdbControllerTests()
        {
            _config = new Mock<IManagerConfiguration>();
            _config.Setup(c => c.LocalIpAddress).Returns("localhost");
            _config.Setup(c => c.ListeningPort).Returns(9876);
            _config.Setup(c => c.AppiumLogFilePath).Returns("appiumLogs");
            _config.Setup(c => c.IdeviceSyslogFolderPath).Returns("syslogs");
        }

        [Fact]
        public void AdbCommand_NullRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.Command(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NullRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.ShellAdbCommand(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_NullDeviceIdAndCommand_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.Command(new AdbCommand());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NullDeviceIdAndCommand_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.ShellAdbCommand(new AdbCommand());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_NullDeviceIdRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);
            var result = adbController.Command(new AdbCommand {Command = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NullDeviceIdRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);
            var result = adbController.ShellAdbCommand(new AdbCommand {Command = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_NullCommandRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.Command(new AdbCommand {AndroidDeviceId = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NullCommandRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.ShellAdbCommand(new AdbCommand {AndroidDeviceId = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_NonExistentDeviceId_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(null));

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.Command(new AdbCommand {AndroidDeviceId = "test", Command = "test"});


            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NonExistentDeviceId_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(null));

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.ShellAdbCommand(new AdbCommand {AndroidDeviceId = "test", Command = "test"});


            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_NotAndroidDeviceType_BadRequest()
        {
            var device = new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.Command(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void ShellAdbCommand_NotAndroidDeviceType_BadRequest()
        {
            var device = new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var adbController = new AdbController(restClientMock.Object, Logger, _externalProcesses);

            var result = adbController.ShellAdbCommand(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdbCommand_ValidRequest_CorrectOutput()
        {
            var device =
                new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.Android, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var externalProcessesMock = new Mock<IExternalProcesses>();
            const string output = "output";
            externalProcessesMock
                .Setup(e => e.RunProcessAndReadOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(output);

            var adbController = new AdbController(restClientMock.Object, Logger, externalProcessesMock.Object);

            var result = adbController.Command(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(output, viewResult.Value);
        }

        [Fact]
        public void ShellAdbCommand_ValidRequest_CorrectOutput()
        {
            var device =
                new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.Android, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var externalProcessesMock = new Mock<IExternalProcesses>();
            const string output = "output";
            externalProcessesMock
                .Setup(e => e.RunProcessAndReadOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(output);

            var adbController = new AdbController(restClientMock.Object, Logger, externalProcessesMock.Object);

            var result = adbController.ShellAdbCommand(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(output, viewResult.Value);
        }

        [Fact]
        public void AdbCommand_ValidRequestExternalProcessThrowsException_ErrorResponse()
        {
            var device =
                new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.Android, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var externalProcessesMock = new Mock<IExternalProcesses>();
            externalProcessesMock
                .Setup(e => e.RunProcessAndReadOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws<Exception>();

            var adbController = new AdbController(restClientMock.Object, Logger, externalProcessesMock.Object);

            var result = adbController.Command(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void ShellAdbCommand_ValidRequestExternalProcessThrowsException_ErrorResponse()
        {
            var device =
                new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.Android, DeviceStatus.Online);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevice(It.IsAny<string>())).Returns(Task.FromResult<Device>(device));

            var externalProcessesMock = new Mock<IExternalProcesses>();
            externalProcessesMock
                .Setup(e => e.RunProcessAndReadOutput(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws<Exception>();

            var adbController = new AdbController(restClientMock.Object, Logger, externalProcessesMock.Object);

            var result = adbController.ShellAdbCommand(new AdbCommand {AndroidDeviceId = device.Id, Command = "test"});

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }
    }
}
