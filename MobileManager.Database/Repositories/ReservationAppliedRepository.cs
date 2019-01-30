using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.Reservations;

namespace MobileManager.Database.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Reservation applied repository.
    /// </summary>
    public class ReservationAppliedRepository : IRepository<ReservationApplied>
    {
        private readonly GeneralDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Database.Repositories.ReservationAppliedRepository"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public ReservationAppliedRepository(GeneralDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the specified reservationApplied.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="reservationApplied">Reservation applied.</param>
        public void Add(ReservationApplied reservationApplied)
        {
            _context.ReservationsApplied.Add(reservationApplied);
            _context.SaveChanges();
        }


        /// <inheritdoc />
        public void Add(IEnumerable<ReservationApplied> entities)
        {
            _context.ReservationsApplied.AddRange(entities);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        /// <summary>
        /// Find the specified id.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="id">Identifier.</param>
        public ReservationApplied Find(string id)
        {
            var reservationApplied = _context.ReservationsApplied.Include(r => r.ReservedDevices)
                .FirstOrDefault(r => r.Id == id);

            return reservationApplied;
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates Applied reservation
        /// </summary>
        /// <param name="reservationApplied">Applied reservation</param>
        public void Update(ReservationApplied reservationApplied)
        {
            _context.Update(reservationApplied);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<ReservationApplied> GetAll()
        {
            var reservationAppliedList = _context.ReservationsApplied.Include(r => r.ReservedDevices)
                .OrderBy(r => r.Id).ToList();

            return reservationAppliedList;
        }

        /// <inheritdoc />
        /// <summary>
        /// Remove the specified id.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="id">Identifier.</param>
        public bool Remove(string id)
        {
            var reservationApplied = Find(id);
            if (reservationApplied == null)
            {
                return false;
            }

            _context.Remove(reservationApplied);
            _context.SaveChanges();


            return true;
        }
    }
}
