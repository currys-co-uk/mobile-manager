using System.Collections.Generic;
using System.Threading.Tasks;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Devices.Interfaces;
using MobileManager.Services.Interfaces;

namespace MobileManager.Utils
{
    /// <summary>
    /// Device utils.
    /// </summary>
    public interface IDeviceUtils
    {
        /// <summary>
        /// Locks the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="restClient">Rest client.</param>
        /// <param name="appiumService">Appium service.</param>
        Task<Device> LockDevice(string deviceId, IRestClient restClient,
            IAppiumService appiumService);

        /// <summary>
        /// Unlocks the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="restClient">Rest client.</param>
        /// <param name="appiumService">Appium service.</param>
        Task<Device> UnlockDevice(string deviceId, IRestClient restClient,
            IAppiumService appiumService);

        /// <summary>
        /// Finds the matching device.
        /// </summary>
        /// <returns>The matching device.</returns>
        /// <param name="requestedDevice">Requested device.</param>
        /// <param name="restClient">Rest client.</param>
        Task<Device> FindMatchingDevice(RequestedDevices requestedDevice, IRestClient restClient);

        /// <summary>
        /// Restarts the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        string RestartDevice(IDevice device);

        /// <summary>
        /// Checks status of connected device ids with stored devices in database - changes status accordingly.
        /// </summary>
        /// <param name="checkedDeviceIds">Ids of devices which are currently connected.</param>
        /// <param name="deviceType">Type of devices for which we have ids.</param>
        /// <param name="restClient"><see cref="IRestClient"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task CheckAllDevicesInDevicePoolAreOnline(IReadOnlyCollection<string> checkedDeviceIds,
            DeviceType deviceType, IRestClient restClient);
    }
}
