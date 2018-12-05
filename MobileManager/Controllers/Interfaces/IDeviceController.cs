using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Models.Devices;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Device controller.
    /// </summary>
    public interface IDeviceController
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        [HttpGet]
        IEnumerable<Device> GetAll();

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="id">Identifier.</param>
        [HttpGet("{id}", Name = "getDevice")]
        IActionResult GetById(string id);

        /// <summary>
        /// Create the specified device.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="device">Device.</param>
        [HttpPost]
        IActionResult Create([FromBody] Device device);

        /// <summary>
        /// Update the specified id and device.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="device">Device.</param>
        [HttpPut("{id}")]
        IActionResult Update(string id, [FromBody] Device device);

        /// <summary>
        /// Delete the specified id.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="id">Identifier.</param>
        [HttpDelete("{id}")]
        IActionResult Delete(string id);
    }
}
