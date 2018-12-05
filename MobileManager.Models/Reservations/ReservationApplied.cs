using System;
using System.Collections.Generic;
using MobileManager.Models.Devices;
using MobileManager.Models.Reservations.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Models.Reservations
{
    /// <summary>
    /// Reservation applied.
    /// </summary>
    public class ReservationApplied : IReservation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Models.Reservations.ReservationApplied"/> class.
        /// </summary>
        /*
         * EF (Entity Framework) requires that a parameterless constructor be declared
         */
        public ReservationApplied()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Models.Reservations.ReservationApplied"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public ReservationApplied(String id)
        {
            Id = id;
            DateCreated = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>The date created.</value>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the reserved devices.
        /// </summary>
        /// <value>The reserved devices.</value>
        //[Required]  //<======= Forces Cascade delete
        //[ForeignKey("ReservationAppliedRefId")]
        public List<ReservedDevice> ReservedDevices { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:MobileManager.Models.Reservations.ReservationApplied"/> is available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        public Boolean Available { get; set; }

        /// <summary>
        /// Gets or sets the failed to apply counter.
        /// </summary>
        /// <value>The failed to apply.</value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int FailedToApply { get; set; }
    }
}