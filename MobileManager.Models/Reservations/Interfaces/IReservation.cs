using System;

namespace MobileManager.Models.Reservations.Interfaces
{
    /// <summary>
    /// Reservation.
    /// </summary>
    public interface IReservation
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>The date created.</value>
        DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:MobileManager.Models.Reservations.Interfaces.IReservation"/> is available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        Boolean Available { get; set; }

        /// <summary>
        /// Gets or sets the failed to apply counter.
        /// </summary>
        /// <value>The failed to apply.</value>
        int FailedToApply { get; set; }
    }
}
