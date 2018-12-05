using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Models.Reservations;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Reservation queue controller.
    /// </summary>
    public interface IReservationQueueController
    {
        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="id">Identifier.</param>
        [HttpGet("{id}", Name = "getReservation")]
        IActionResult GetById(string id);

        /// <summary>
        /// Creates the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="reservation">Reservation.</param>
        [HttpPost]
        Task<IActionResult> CreateAsync([FromBody] Reservation reservation);

        /// <summary>
        /// Delete the specified id.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="id">Identifier.</param>
        [HttpDelete("{id}")]
        IActionResult Delete(string id);
    }
}
