using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Appium log controller.
    /// </summary>
    public interface IAppiumLogController
    {
        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="id">Identifier.</param>
        [HttpGet("{id}")]
        Task<IActionResult> GetById(string id);

        /// <summary>
        /// Delete the specified id.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="id">Identifier.</param>
        [HttpDelete("{id}")]
        Task<IActionResult> Delete(string id);
    }
}
