using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Appium;
using MobileManager.Configuration;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Reservations;
using Moq;
using Xunit;

namespace MobileManagerTests.Controllers
{
    public class AdminControllerTests
    {
        private static readonly IManagerLogger Logger = new Mock<IManagerLogger>().Object;

        [Fact]
        public void GetAllRepositoriesAsync_ValidResult()
        {
            var restClientMock = new Mock<IRestClient>();
            restClientMock.Setup(r => r.GetDevices()).Returns(Task.FromResult(GetTestDevices()));
            restClientMock.Setup(r => r.GetReservations()).Returns(Task.FromResult(GetTestReservations()));
            restClientMock.Setup(r => r.GetAppliedReservations())
                .Returns(Task.FromResult(GetTestReservationsApplied()));
            restClientMock.Setup(r => r.GetManagerConfiguration())
                .Returns(Task.FromResult<IManagerConfiguration>(new ManagerConfiguration()));
            restClientMock.Setup(r => r.GetAppiumProcesses()).Returns(Task.FromResult(GetTestAppiumProcess()));

            var adminController = new AdminController(restClientMock.Object, Logger);

            var result = adminController.GetAllRepositoriesAsync().Result;

            Assert.IsType<JsonResult>(result);
        }


        #region privates

        private static readonly Device Device1 =
            new DeviceFactory().NewDevice("111", "device_111", true, DeviceType.IOS, DeviceStatus.Online);

        private static readonly Device Device2 =
            new DeviceFactory().NewDevice("222", "device_222", true, DeviceType.Android, DeviceStatus.Online);

        private static readonly Device Device3 =
            new DeviceFactory().NewDevice("333", "device_333", true, DeviceType.Android, DeviceStatus.Online);

        private IEnumerable<Device> GetTestDevices()
        {
            return new List<Device>
            {
                Device1,
                Device2,
                Device3
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

        private IEnumerable<ReservationApplied> GetTestReservationsApplied()
        {
            return new List<ReservationApplied>
            {
                _reservationApplied1,
                _reservationApplied2
            };
        }

        private IEnumerable<AppiumProcess> GetTestAppiumProcess()
        {
            return new List<AppiumProcess>()
            {
                new AppiumProcess(Device1.Id, "1111", "1112", 111, "1234", "12345"),
                new AppiumProcess(Device1.Id, "2222", "2223", 222, "2345", "23456")
            };
        }

        #endregion
    }
}
