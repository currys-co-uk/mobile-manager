using System;
using System.Threading.Tasks;

namespace MobileManager.Services.Interfaces
{
    /// <summary>
    /// Appium service.
    /// </summary>
    public interface IAppiumService
    {
        /// <summary>
        /// Starts the appium for device identifier.
        /// </summary>
        /// <returns>The appium for device identifier.</returns>
        /// <param name="deviceId">Device identifier.</param>
        Task<String> StartAppiumForDeviceId(String deviceId);

        /// <summary>
        /// Stops the appium for device identifier async.
        /// </summary>
        /// <returns>The appium for device identifier async.</returns>
        /// <param name="deviceId">Device identifier.</param>
        Task<bool> StopAppiumForDeviceIdAsync(String deviceId);
    }
}
