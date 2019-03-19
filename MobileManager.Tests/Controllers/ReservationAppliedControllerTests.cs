using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Reservations;
using MobileManager.Services;
using MobileManager.Services.Interfaces;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace MobileManagerTests.Controllers
{
    public class ReservationAppliedControllerTests
    {
        private static readonly Device Device1 =
            new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

        private static readonly Device Device2 =
            new DeviceFactory().NewDevice("222", "device_222", true, DeviceType.Android, DeviceStatus.Online);

        private readonly ReservationApplied _reservationApplied1 = new ReservationApplied()
        {
            ReservedDevices = new List<ReservedDevice>()
            {
                new ReservedDevice(Device1)
            }
        };

        private readonly ReservationApplied _reservationApplied2 = new ReservationApplied()
        {
            ReservedDevices = new List<ReservedDevice>()
            {
                new ReservedDevice(Device2)
            }
        };

        private readonly IExternalProcesses _externalProcesses = new Mock<IExternalProcesses>().Object;
        private static readonly IManagerLogger Logger = new Mock<IManagerLogger>().Object;
        private readonly RestClient _restClient;
        private readonly AppiumService _appiumService;

        private readonly string _httpLocalhost;
        private readonly Mock<IManagerConfiguration> _config;

        public ReservationAppliedControllerTests()
        {
            _config = new Mock<IManagerConfiguration>();
            _config.Setup(c => c.LocalIpAddress).Returns("localhost");
            _config.Setup(c => c.ListeningPort).Returns(9876);
            _config.Setup(c => c.AppiumLogFilePath).Returns("appiumLogs");
            _config.Setup(c => c.IdeviceSyslogFolderPath).Returns("syslogs");

            _restClient = new RestClient(_config.Object, Logger);
            _appiumService = new AppiumService(_config.Object, Logger, _externalProcesses);

            _httpLocalhost = $"http://{_config.Object.LocalIpAddress}:{_config.Object.ListeningPort}";
        }

        [Fact]
        public void GetAllAppliedReservations()
        {
            var appliedReservations = new List<ReservationApplied> {_reservationApplied1, _reservationApplied2};
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(appliedReservations);

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.GetAllAppliedReservations();

            // Assert
            var viewResult = Assert.IsType<List<ReservationApplied>>(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(_reservationApplied1, viewResult);
            Assert.Contains(_reservationApplied2, viewResult);
        }

        [Fact]
        public void GetAllAppliedReservations_empty()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(new List<ReservationApplied>());

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.GetAllAppliedReservations();

            // Assert
            var viewResult = Assert.IsType<List<ReservationApplied>>(result);
            Assert.Empty(viewResult);
        }

        [Fact]
        public void GetAppliedReservationsById()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Find(_reservationApplied1.Id)).Returns(_reservationApplied1);

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.GetById(_reservationApplied1.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(_reservationApplied1, viewResult.Value);
        }

        [Fact]
        public void GetAppliedReservationsById_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Find(_reservationApplied1.Id)).Returns((ReservationApplied) null);

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.GetById(_reservationApplied1.Id);

            // Assert
            var viewResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Reservation not found in the database.", viewResult.Value);
        }


        [Fact]
        public void CreateAppliedReservation()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Add(_reservationApplied1));

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.Create(_reservationApplied1);

            // Assert
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(_reservationApplied1, viewResult.Value);
        }

        [Fact]
        public void CreateAppliedReservation_empty()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Add((ReservationApplied) null));

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.Create(null);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Reservation is empty.", viewResult.Value);
        }

        [Fact]
        public void CreateAppliedReservation_requestedDevices_empty()
        {
            var reservationApplied = _reservationApplied1;
            reservationApplied.ReservedDevices.Clear();

            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Add(reservationApplied));

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.Create(_reservationApplied1);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RequestedDevices property is empty.", viewResult.Value);
        }

        [Fact]
        public void CreateAppliedReservation_requestedDevices_null()
        {
            var reservationApplied = _reservationApplied1;
            reservationApplied.ReservedDevices = null;

            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Add(reservationApplied));

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = controller.Create(_reservationApplied1);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RequestedDevices property is empty.", viewResult.Value);
        }

        [Fact]
        public async void DeleteAppliedReservationAsync()
        {
            // Arrange
            var unlockedDevice = Device1;
            unlockedDevice.Available = true;
            unlockedDevice.AppiumEndpoint = "";

            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Find(_reservationApplied1.Id)).Returns(_reservationApplied1);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, $"{_httpLocalhost}/api/v1/device/{Device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(unlockedDevice));
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device/{Device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(Device1));

            var mockAppiumService = new Mock<IAppiumService>();
            mockAppiumService.Setup(appiumService => appiumService.StopAppiumForDeviceIdAsync(Device1.Id))
                .Returns(Task.FromResult(true));

            var controller = new ReservationsAppliedController(mockRepository.Object,
                new RestClient(_config.Object, new HttpClient(mockHttp), Logger), mockAppiumService.Object, Logger,
                _externalProcesses);

            // Act
            var result = await controller.DeleteAsync(_reservationApplied1.Id);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async void DeleteAppliedReservationAsync_failed_to_unlock_device()
        {
            // Arrange
            var unlockedDevice = Device1;
            unlockedDevice.Available = false;
            unlockedDevice.AppiumEndpoint = "";

            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Find(_reservationApplied1.Id)).Returns(_reservationApplied1);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Put, $"{_httpLocalhost}/api/v1/device/{Device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(unlockedDevice));
            mockHttp.When(HttpMethod.Get, $"{_httpLocalhost}/api/v1/device/{Device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(Device1));

            var controller = new ReservationsAppliedController(mockRepository.Object,
                new RestClient(_config.Object, new HttpClient(mockHttp), Logger), _appiumService, Logger,
                _externalProcesses);

            // Act
            var result = await controller.DeleteAsync(_reservationApplied1.Id);

            // Assert
            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
            Assert.Equal("Failed to unlock device id: " + Device1.Id + " from reservation.", viewResult.Value);
        }

        [Fact]
        public async void DeleteAppliedReservationAsync_id_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<ReservationApplied>>();
            mockRepository.Setup(mpr => mpr.Find(_reservationApplied1.Id)).Returns((ReservationApplied) null);

            var controller =
                new ReservationsAppliedController(mockRepository.Object, _restClient, _appiumService, Logger,
                    _externalProcesses);

            // Act
            var result = await controller.DeleteAsync(_reservationApplied1.Id);

            // Assert
            var viewResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Reservation not found in database.", viewResult.Value);
        }
    }
}
