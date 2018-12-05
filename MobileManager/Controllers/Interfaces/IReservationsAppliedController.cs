using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Models.Reservations;

namespace MobileManager.Controllers.Interfaces
{
    /// <summary>
    /// Reservations applied controller.
    /// </summary>
    public interface IReservationsAppliedController
    {
        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="id">Identifier.</param>
        [HttpGet("{id}", Name = "getReservation")]
        IActionResult GetById(string id);

        /// <summary>
        /// Create the specified reservation.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="reservation">Reservation.</param>
        [HttpPost]
        IActionResult Create([FromBody] ReservationApplied reservation);

        /// <summary>
        /// Deletes the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="id">Identifier.</param>
        [HttpDelete("{id}")]
        Task<IActionResult> DeleteAsync(string id);
    }
}
