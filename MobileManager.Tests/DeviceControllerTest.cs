using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Services;
using MobileManager.Utils;
using Moq;
using Xunit;

namespace MobileManagerTests
{
    public class DeviceControllerTest
    {
        private readonly Device _device1 =
            new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

        private readonly Device _device2 =
            new DeviceFactory().NewDevice("222", "device_222", true, DeviceType.Android, DeviceStatus.Online);

        private readonly Device _device3 =
            new DeviceFactory().NewDevice("333", "device_333", true, DeviceType.Android, DeviceStatus.Online);

        private List<Device> GetTestDevices()
        {
            return new List<Device>
            {
                _device1,
                _device2,
                _device3
            };
        }

        private readonly IExternalProcesses _externalProcesses = new Mock<IExternalProcesses>().Object;

        [Fact]
        public void GetAllDevices()
        {
            // Arrange
            var testDevices = GetTestDevices();
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(testDevices);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);

            // Act
            var result = controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<List<Device>>(result);
            Assert.Equal(testDevices.Count(), result.Count());
            Assert.Contains(_device1, viewResult);
            Assert.Contains(_device2, viewResult);
            Assert.Contains(_device3, viewResult);
        }

        [Fact]
        public void GetAllDevices_empty()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(new List<Device>());

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<List<Device>>(result);
            Assert.Empty(viewResult);
        }

        [Fact]
        public void GetById()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetById(_device1.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(_device1, viewResult.Value);
        }

        [Fact]
        public void GetById_empty_input()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(""))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetById("");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetById_id_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetById(_device2.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void CreateDevice()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(_device1));

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(_device1);

            // Assert
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(_device1, viewResult.Value);
        }

        [Fact]
        public void CreateDevice_null()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add((Device) null));

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateDevice_conflict()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(_device1));
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(_device1);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, viewResult.StatusCode);
        }

        [Fact]
        public void CreateDevice_DeviceIdEmpty()
        {
            var device = new Device("", _device1.Name, _device1.Available, _device1.Type, _device1.Status);
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(device));

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(device);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateDevice_DeviceNameEmpty()
        {
            var device = new Device(_device1.Id, "", _device1.Available, _device1.Type, _device1.Status);
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(device));

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(device);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateDevice_FailedToAdd()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(_device1)).Throws(new Exception());

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.Create(_device1);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void UpdateDevice()
        {
            // Arrange
            Device device = _device1;
            device.Name = "updated name";

            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act

            var result = controller.Update(_device1.Id, device);

            // Assert
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(device, viewResult.Value);
        }

        [Fact]
        public void UpdateDevice_null_device()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act

            var result = controller.Update(_device1.Id, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateDevice_different_ids()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act

            var result = controller.Update(_device1.Id, _device2);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateDevice_notFound()
        {
            // Arrange
            Device device = _device1;
            device.Name = "updated name";
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act

            var result = controller.Update(_device1.Id, device);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void UpdateDevice_FailedToUpdate()
        {
            // Arrange
            Device device = _device1;
            device.Name = "updated name";

            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);
            mockRepository.Setup(repo => repo.Update(_device1))
                .Throws(new Exception());

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act

            var result = controller.Update(_device1.Id, device);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void RemoveDevice()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(_device1));
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            controller.Create(_device1);

            // Act
            var result = controller.Delete(_device1.Id);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void RemoveDevice_DeviceLocked()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Locked);
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(device));
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            controller.Create(_device1);

            // Act
            var result = controller.Delete(device.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(423, viewResult.StatusCode);
        }

        [Fact]
        public void RemoveDevice_DeviceNotFound()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Locked);
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(device));
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            controller.Create(_device1);

            // Act
            var result = controller.Delete(device.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void RemoveDevice_FailedToRemove()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Add(_device1));
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);
            mockRepository.Setup(repo => repo.Remove(_device1.Id)).Throws(new Exception());

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            controller.Create(_device1);

            // Act
            var result = controller.Delete(_device1.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void GetPropertiesById_Found()
        {
            var device = new Device(_device1.Id, _device1.Name, true, _device1.Type, DeviceStatus.Online);
            var deviceProperties = new List<DeviceProperties>
            {
                new DeviceProperties("key1", "value1"), new DeviceProperties("key2", "value2")
            };
            device.Properties = deviceProperties;

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetPropertiesById(device.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(deviceProperties, viewResult.Value);
        }

        [Fact]
        public void GetPropertiesById_DeviceNotFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetPropertiesById(_device1.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetAllPropertiesKeys()
        {
            var device = new Device(_device1.Id, _device1.Name, true, _device1.Type, DeviceStatus.Online);
            var device2 = new Device(_device1.Id, _device1.Name, true, _device1.Type, DeviceStatus.Online);
            var deviceProperties = new List<DeviceProperties>
            {
                new DeviceProperties("key1", "value1"),
                new DeviceProperties("key2", "value2")
            };
            device.Properties = deviceProperties;
            device2.Properties = deviceProperties;

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.GetAll())
                .Returns(new List<Device> {device, device2});

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetAllPropertiesKeys();

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(new List<string> {"key1", "key2"}, viewResult.Value);
        }

        [Fact]
        public void GetSeleniumConfigById_Found_iOS()
        {
            var device = new Device(_device1.Id, _device1.Name, true, DeviceType.IOS, DeviceStatus.Online);
            const string version = "12.1";

            var deviceProperties = new List<DeviceProperties>
            {
                new DeviceProperties("ProductVersion", version),
                new DeviceProperties("Version", "11.1")
            };
            device.Properties = deviceProperties;

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetSeleniumConfigById(device.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Contains(_device1.Id, viewResult.Value.ToString());
            Assert.Contains($"deviceVersion = {version}", viewResult.Value.ToString());
            Assert.Contains("teamId", viewResult.Value.ToString());
        }

        [Fact]
        public void GetSeleniumConfigById_Found_Android()
        {
            var device = new Device(_device1.Id, _device1.Name, true, DeviceType.Android, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetSeleniumConfigById(device.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Contains(_device1.Id, viewResult.Value.ToString());
            Assert.Contains("platformName = android", viewResult.Value.ToString());
        }

        [Fact]
        public void GetSeleniumConfigById_Unspecified()
        {
            var device = new Device(_device1.Id, _device1.Name, true, DeviceType.Unspecified, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetSeleniumConfigById(device.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Unsupported device type", viewResult.Value);
        }

        [Fact]
        public void GetSeleniumConfigById_unsupportedDeviceType()
        {
            var device = new Device(_device1.Id, _device1.Name, true, (DeviceType) 99, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetSeleniumConfigById(device.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Unsupported device type", viewResult.Value);
        }

        [Fact]
        public void GetSeleniumConfigById_DeviceNotFound()
        {
            var device = new Device(_device1.Id, _device1.Name, true, DeviceType.IOS, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetSeleniumConfigById(device.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void RestartDevice_Ok()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            mockDeviceUtils.Setup(x => x.RestartDevice(_device1)).Returns(string.Empty);

            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                mockDeviceUtils.Object, _externalProcesses);
            // Act
            var result = controller.RestartDevice(_device1.Id);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void RestartDevice_DeviceNotFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.RestartDevice(_device1.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void RestartDevice_DeviceNotAvailable()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Locked);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.RestartDevice(device.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(423, viewResult.StatusCode);
        }

        [Fact]
        public void RestartDevice_RestartFailed()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns(_device1);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            mockDeviceUtils.Setup(x => x.RestartDevice(_device1)).Returns("ERROR");

            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                mockDeviceUtils.Object, _externalProcesses);
            // Act
            var result = controller.RestartDevice(_device1.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void GetDeviceScreenshotById_DeviceNotFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(_device1.Id))
                .Returns((Device) null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object,
                _externalProcesses);
            // Act
            var result = controller.GetDeviceScreenshotById(_device1.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetDeviceScreenshotById_DeviceOffline()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Offline);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.LoadScreenshotForOfflineDevice(device))
                .Returns(new FileStreamResult(File.Create(Guid.NewGuid().ToString()), "text/html"));

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);
            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public void GetDeviceScreenshotById_DeviceFailedToInitialize()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.FailedToInitialize);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.LoadScreenshotForOfflineDevice(device))
                .Returns(new FileStreamResult(File.Create(Guid.NewGuid().ToString()), "text/html"));

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);
            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public void GetDeviceScreenshotById_DeviceLockedOffice()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.LockedOffline);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.LoadScreenshotForOfflineDevice(device))
                .Returns(new FileStreamResult(File.Create(Guid.NewGuid().ToString()), "text/html"));

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);
            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public void GetDeviceScreenshotById_DeviceOfflineFailedToGetDefault()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Offline);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();
            mockScreenshotService.Setup(x => x.LoadScreenshotForOfflineDevice(device)).Throws(new Exception());

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);
            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void GetDeviceScreenshotById_ScreenshotLocked_ThrowsTimeoutException()
        {
            var device = new Device(_device1.Id, _device1.Name, false, _device1.Type, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);

            controller.ScreenshotLocked.Add(device.Id);
            controller.ScreenshotLockedTimeout = 500;

            // Act
            // Assert
            var start = DateTime.Now;
            Assert.Throws<TimeoutException>(() => controller.GetDeviceScreenshotById(device.Id));
            Assert.True(start + TimeSpan.FromMilliseconds(controller.ScreenshotLockedTimeout) <
                        DateTime.Now + TimeSpan.FromMilliseconds(1000));
            Assert.Contains(device.Id, controller.ScreenshotLocked);
        }

        [Fact]
        public void GetDeviceScreenshotById_iOS_FailedToTakeScreenshot()
        {
            var device = new Device(_device1.Id, _device1.Name, false, DeviceType.IOS, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.TakeScreenshotIosDevice(device)).Throws(new Exception());

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);

            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
            Assert.DoesNotContain(device.Id, controller.ScreenshotLocked);
        }

        [Fact]
        public void GetDeviceScreenshotById_Android_FailedToTakeScreenshot()
        {
            var device = new Device(_device1.Id, _device1.Name, false, DeviceType.Android, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.TakeScreenshotAndroidDevice(device)).Throws(new Exception());

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);

            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
            Assert.DoesNotContain(device.Id, controller.ScreenshotLocked);
        }

        [Fact]
        public void GetDeviceScreenshotById_UnspecifiedDeviceType()
        {
            var device = new Device(_device1.Id, _device1.Name, false, DeviceType.Unspecified, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.TakeScreenshotAndroidDevice(device)).Throws(new Exception());

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);

            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.DoesNotContain(device.Id, controller.ScreenshotLocked);
        }

        [Fact]
        public void GetDeviceScreenshotById_UnknownDeviceType()
        {
            var device = new Device(_device1.Id, _device1.Name, false, (DeviceType) 99, DeviceStatus.Online);

            // Arrange
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(repo => repo.Find(device.Id))
                .Returns(device);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var mockDeviceUtils = new Mock<IDeviceUtils>();
            var mockScreenshotService = new Mock<IScreenshotService>();

            mockScreenshotService.Setup(x => x.TakeScreenshotAndroidDevice(device)).Throws(new Exception());

            var controller = new DevicesController(
                mockRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockDeviceUtils.Object,
                mockScreenshotService.Object,
                _externalProcesses);

            // Act
            var result = controller.GetDeviceScreenshotById(device.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.DoesNotContain(device.Id, controller.ScreenshotLocked);
        }
    }
}