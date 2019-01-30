using System.Collections.Generic;

namespace MobileManager.Models.Reservations.Interfaces
{
    public interface IReservationsRepository
    {
        void Add(Reservation reservation);
        IEnumerable<Reservation> getAll();
        Reservation Find(string id);
        void Remove(string id);
    }
}
