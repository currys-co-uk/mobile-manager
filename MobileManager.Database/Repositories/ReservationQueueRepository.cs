using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.Reservations;

namespace MobileManager.Database.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Reservation queue repository.
    /// </summary>
    public class ReservationQueueRepository : IRepository<Reservation>
    {
        private readonly GeneralDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Database.Repositories.ReservationQueueRepository"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public ReservationQueueRepository(GeneralDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the specified reservation.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="reservation">Reservation.</param>
        public void Add(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            _context.SaveChanges();
        }


        /// <inheritdoc />
        public void Add(IEnumerable<Reservation> entities)
        {
            _context.Reservations.AddRange(entities);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        /// <summary>
        /// Find the specified id.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="id">Identifier.</param>
        public Reservation Find(string id)
        {
            var reservationQueued = _context.Reservations.Include(r => r.RequestedDevices)
                .FirstOrDefault(r => r.Id == id);

            return reservationQueued;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<Reservation> GetAll()
        {
            var reservationsList = _context.Reservations.OrderBy(r => r.Id).Include(r => r.RequestedDevices)
                .ThenInclude(r => r.Properties);

            return reservationsList.ToList();
        }

        /// <inheritdoc />
        /// <summary>
        /// Remove the specified id.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="id">Identifier.</param>
        public bool Remove(string id)
        {
            var reservation = Find(id);
            if (reservation == null)
            {
                return false;
            }

            _context.Remove(reservation);
            _context.SaveChanges();

            return true;
        }


        /// <inheritdoc />
        /// <summary>
        /// Update the specified reservationUpdated.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="reservationUpdated">Reservation updated.</param>
        public void Update(Reservation reservationUpdated)
        {
            if (Remove(reservationUpdated.Id))
            {
                Add(reservationUpdated);
            }
        }
    }
}
