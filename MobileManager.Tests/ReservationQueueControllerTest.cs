using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Reservations;
using MobileManager.Services;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace MobileManagerTests
{
    public class ReservationQueueControllerTest
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

        private readonly Reservation _reservation1 = new Reservation()
        {
            RequestedDevices = new List<RequestedDevices>
            {
                new RequestedDevices()
                {
                    DeviceId = "111",
                    DeviceType = DeviceType.IOS
                }
            }
        };

        private readonly Reservation _reservation2 = new Reservation()
        {
            RequestedDevices = new List<RequestedDevices>
            {
                new RequestedDevices()
                {
                    DeviceId = "111",
                    DeviceType = DeviceType.IOS
                }
            }
        };


        private IEnumerable<Reservation> GetTestReservations()
        {
            return new List<Reservation>
            {
                _reservation1,
                _reservation2
            };
        }

        private readonly IExternalProcesses _externalProcesses = new Mock<IExternalProcesses>().Object;

        private static readonly IManagerLogger Logger = new ManagerLogger();
        private readonly RestClient _restClient;
        private readonly string _httpLocalhost;

        private readonly Mock<IManagerConfiguration> _config;

        public ReservationQueueControllerTest()
        {
            _config = new Mock<IManagerConfiguration>();
            _config.Setup(c => c.LocalIpAddress).Returns("localhost");
            _config.Setup(c => c.ListeningPort).Returns(9876);
            _config.Setup(c => c.AppiumLogFilePath).Returns("appiumLogs");
            _config.Setup(c => c.IdeviceSyslogFolderPath).Returns("syslogs");

            _restClient = new RestClient(_config.Object, Logger);

            _httpLocalhost =
                $"http://{_config.Object.LocalIpAddress}:{_config.Object.ListeningPort}";
        }

        [Fact]
        public void GetAllReservations()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(GetTestReservations());

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<List<Reservation>>(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(_reservation1, viewResult);
            Assert.Contains(_reservation2, viewResult);
        }

        [Fact]
        public void GetAllReservations_empty()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.GetAll()).Returns(new List<Reservation>());

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetAll();

            // Assert
            Assert.IsType<List<Reservation>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetReservation_reservation1()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Find(_reservation1.Id)).Returns(_reservation1);

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetById(_reservation1.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(_reservation1, viewResult.Value);
        }

        [Fact]
        public void GetReservation_reservation2()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Find(_reservation2.Id)).Returns(_reservation2);

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetById(_reservation2.Id);

            // Assert
            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(_reservation2, viewResult.Value);
        }

        [Fact]
        public void GetReservation_id_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Find(_reservation1.Id)).Returns((Reservation) null);

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetById(_reservation1.Id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetReservation_empty_Input()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Find("")).Returns((Reservation) null);

            var controller = new ReservationsQueueController(mockRepository.Object, _restClient, Logger, _externalProcesses);

            // Act
            var result = controller.GetById("");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void CreateReservation_reservation1()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(_reservation1));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));
            mockHttp.When($"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(_reservation1);

            // Assert
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(_reservation1, viewResult.Value);
        }

        [Fact]
        public async void CreateReservation_device_with_id_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(_reservation1));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device2}));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(_reservation1);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(
                $"Requested device(s) not found in the device pool: [\"{{\\\"DeviceId\\\":\\\"{_device1.Id}\\\",\\\"DeviceType\\\":\\\"{_device1.Type}\\\"}}\"]",
                viewResult.Value);
        }

        [Fact]
        public async void CreateReservation_empty_requested_devices()
        {
            var reservation = _reservation1;
            reservation.RequestedDevices.Clear();

            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(reservation));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(reservation);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RequestedDevices property is empty.", viewResult.Value);
        }

        [Fact]
        public async void CreateReservation_null_requested_devices()
        {
            var reservation = _reservation1;
            reservation.RequestedDevices = null;

            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(reservation));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(reservation);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RequestedDevices property is empty.", viewResult.Value);
        }

        [Fact]
        public async void CreateReservation_duplicated_deviceId()
        {
            var reservationDuplicateDeviceId = new Reservation()
            {
                RequestedDevices = new List<RequestedDevices>
                {
                    new RequestedDevices()
                    {
                        DeviceId = "111",
                        DeviceType = DeviceType.IOS
                    },
                    new RequestedDevices()
                    {
                        DeviceId = "111",
                        DeviceType = DeviceType.IOS
                    }
                }
            };
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(reservationDuplicateDeviceId));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(reservationDuplicateDeviceId);

            // Assert
            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RequestedDevices property contains duplicate DeviceId.", viewResult.Value);
        }

        [Fact]
        public async void DeleteReservation()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(_reservation1));
            mockRepository.Setup(mpr => mpr.Find(_reservation1.Id)).Returns(_reservation1);

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));
            mockHttp.When($"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(_reservation1);

            // Assert created
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(_reservation1, viewResult.Value);

            var resultDelete = controller.Delete(_reservation1.Id);

            // Assert deleted
            Assert.IsType<OkResult>(resultDelete);
            Assert.Empty(controller.GetAll());
        }

        [Fact]
        public async void DeleteReservation_id_notFound()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<Reservation>>();
            mockRepository.Setup(mpr => mpr.Add(_reservation1));
            mockRepository.Setup(mpr => mpr.Find(_reservation1.Id)).Returns((Reservation) null);

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(mrc => mrc.GetDevices()).Returns(new Task<IEnumerable<Device>>(GetTestDevices));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{_httpLocalhost}/api/v1/device").Respond("application/json",
                JsonConvert.SerializeObject(new List<Device> {_device1}));
            mockHttp.When($"{_httpLocalhost}/api/v1/device/{_device1.Id}")
                .Respond("application/json", JsonConvert.SerializeObject(_device1));

            var restClient = new RestClient(_config.Object,
                new HttpClient(mockHttp), Logger);
            var controller = new ReservationsQueueController(mockRepository.Object, restClient, Logger, _externalProcesses);

            // Act
            var result = await controller.CreateAsync(_reservation1);

            // Assert created
            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(_reservation1, viewResult.Value);

            var resultDelete = controller.Delete(_reservation1.Id);

            // Assert deleted
            var viewResultDelete = Assert.IsType<NotFoundObjectResult>(resultDelete);
            Assert.Equal("Reservation not found in the database.", viewResultDelete.Value);
        }
    }
}