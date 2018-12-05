using MobileManager.Controllers;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;

namespace MobileManagerTests
{
    public class DeviceControllerTest
    {
        private readonly Device _device1 = new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);
        private readonly Device _device2 = new DeviceFactory().NewDevice("222", "device_222", true, DeviceType.Android, DeviceStatus.Online);
        private readonly Device _device3 = new DeviceFactory().NewDevice("333", "device_333", true, DeviceType.Android, DeviceStatus.Online);

        private List<Device> GetTestDevices()
        {
            return new List<Device>
            {
                _device1,
                _device2,
                _device3
            };
        }

        [Fact]
        public void GetAllDevices()
        {
            // Arrange
            var testDevices = GetTestDevices();
            var mockRepository = new Mock<IRepository<Device>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(testDevices);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);

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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
                          .Returns((Device)null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
                          .Returns((Device)null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            mockRepository.Setup(repo => repo.Add((Device)null));

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
            // Act
            var result = controller.Create(_device1);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, viewResult.StatusCode);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
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
                          .Returns((Device)null);

            var mockLogger = new Mock<IManagerLogger>();
            var mockConfiguration = new Mock<IManagerConfiguration>();
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);
            // Act

            var result = controller.Update(_device1.Id, device);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
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
            var controller = new DevicesController(mockRepository.Object, mockLogger.Object, mockConfiguration.Object);            controller.Create(_device1);

            // Act
            var result = controller.Delete(_device1.Id);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Empty(controller.GetAll());
        }
    }
}