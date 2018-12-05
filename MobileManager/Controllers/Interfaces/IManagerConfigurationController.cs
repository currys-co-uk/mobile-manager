using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Manager configuration controller.
    /// </summary>
    public interface IManagerConfigurationController
    {
        /// <summary>
        /// Get this instance.
        /// </summary>
        /// <returns>The get.</returns>
        [HttpGet]
        IActionResult Get();

        /// <summary>
        /// Update the specified configuration.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="configuration">Configuration.</param>
        [HttpPut]
        IActionResult Update([FromBody] ManagerConfiguration configuration);
    }
}
