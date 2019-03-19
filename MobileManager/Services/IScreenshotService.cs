using Microsoft.AspNetCore.Mvc;
using MobileManager.Models.Devices;

namespace MobileManager.Services
{
    /// <summary>
    /// ScreenshotService handling ios and android screenshots.
    /// </summary>
    public interface IScreenshotService
    {
        /// <summary>
        /// Takes the screenshot ios device.
        /// </summary>
        /// <returns>The screenshot ios device.</returns>
        /// <param name="device">Device.</param>
        FileStreamResult TakeScreenshotIosDevice(Device device);

        /// <summary>
        /// Takes the screenshot android device.
        /// </summary>
        /// <returns>The screenshot android device.</returns>
        /// <param name="device">Device.</param>
        FileStreamResult TakeScreenshotAndroidDevice(Device device);

        /// <summary>
        /// Loads the screenshot for offline device.
        /// </summary>
        /// <returns>The screenshot for offline device.</returns>
        /// <param name="device">Device.</param>
        FileStreamResult LoadScreenshotForOfflineDevice(Device device);
    }
}
