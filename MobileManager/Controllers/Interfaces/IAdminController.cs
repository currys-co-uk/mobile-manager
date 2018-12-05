using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Admin controller.
    /// </summary>
    public interface IAdminController
    {
        /// <summary>
        /// Gets all repositories async.
        /// </summary>
        /// <returns>The all repositories async.</returns>
        [HttpGet]
        Task<IActionResult> GetAllRepositoriesAsync();
    }
}
