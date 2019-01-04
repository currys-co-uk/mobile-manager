using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Appium;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Appium controller.
    /// </summary>
    public interface IAppiumController
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        [HttpGet]
        IEnumerable<AppiumProcess> GetAll();

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="id">Identifier.</param>
        [HttpGet("{id}")]
        IActionResult GetById(string id);

        /// <summary>
        /// Create the specified appiumProcess.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="appiumProcess">Appium process.</param>
        [HttpPost]
        IActionResult Create([FromBody] AppiumProcess appiumProcess);

        /// <summary>
        /// Delete the specified id.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="id">Identifier.</param>
        [HttpDelete("{id}")]
        IActionResult Delete(string id);
    }
}