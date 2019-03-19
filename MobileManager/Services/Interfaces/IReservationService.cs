using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobileManager.Models.Reservations;

namespace MobileManager.Services.Interfaces
{
    /// <summary>
    /// Reservation service.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Runs the async apply reservation task async.
        /// </summary>
        Task RunAsyncApplyReservationTaskAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Loads the reservation queue.
        /// </summary>
        /// <returns>The reservation queue.</returns>
        Task<IEnumerable<Reservation>> LoadReservationQueue();

        /// <summary>
        /// Applies the available reservations.
        /// </summary>
        /// <returns>The available reservations.</returns>
        Task<Boolean> ApplyAvailableReservations();
    }
}
