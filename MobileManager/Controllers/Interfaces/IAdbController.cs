using Microsoft.AspNetCore.Mvc;
using MobileManager.Models.Adb;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Adb controller.
    /// </summary>
    public interface IAdbController
    {
        /// <summary>
        /// Execute basic ADB command on the device.
        /// </summary>
        /// <returns>The command.</returns>
        /// <param name="adbCommand">Adb command.</param>
        [HttpPost]
        IActionResult Command([FromBody] AdbCommand adbCommand);
        
        /// <summary>
        /// Execute shell ADB command on the device.
        /// </summary>
        /// <returns>The command.</returns>
        /// <param name="adbCommand">Adb command.</param>
        [HttpPost]
        IActionResult ShellCommand([FromBody] AdbCommand adbCommand);
    }
}
