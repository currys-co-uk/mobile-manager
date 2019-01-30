using System.Collections.Generic;
using System.Threading.Tasks;
using MobileManager.Appium;
using MobileManager.Configuration.Interfaces;
using MobileManager.Models.Devices;
using MobileManager.Models.Reservations;

namespace MobileManager.Http.Clients.Interfaces
{
    /// <summary>
    /// Rest client.
    /// </summary>
    public interface IRestClient
    {
        /// <summary>
        /// Tries to connect.
        /// </summary>
        /// <returns>The to connect.</returns>
        Task<bool> TryToConnect();

        /// <summary>
        /// Adds the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        Task<Device> AddDevice(Device device);

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>The devices.</returns>
        Task<IEnumerable<Device>> GetDevices();

        /// <summary>
        /// Gets the reservations.
        /// </summary>
        /// <returns>The reservations.</returns>
        Task<IEnumerable<Reservation>> GetReservations();

        /// <summary>
        /// Gets the applied reservations.
        /// </summary>
        /// <returns>The applied reservations.</returns>
        Task<IEnumerable<ReservationApplied>> GetAppliedReservations();

        /// <summary>
        /// Applies the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservation">Reservation.</param>
        Task<ReservationApplied> ApplyReservation(ReservationApplied reservation);

        /// <summary>
        /// Updates the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservation">Reservation.</param>
        Task<Reservation> UpdateReservation(Reservation reservation);

        /// <summary>
        /// Deletes the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservationId">Reservation identifier.</param>
        Task<bool> DeleteReservation(string reservationId);

        /// <summary>
        /// Deletes the applied reservation.
        /// </summary>
        /// <returns>The applied reservation.</returns>
        /// <param name="reservationAppliedId">Reservation applied identifier.</param>
        Task<bool> DeleteAppliedReservation(string reservationAppliedId);

        /// <summary>
        /// Gets the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="id">Identifier.</param>
        Task<Reservation> GetReservation(string id);

        /// <summary>
        /// Gets the applied reservation.
        /// </summary>
        /// <returns>The applied reservation.</returns>
        /// <param name="id">Identifier.</param>
        Task<ReservationApplied> GetAppliedReservation(string id);

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="id">Identifier.</param>
        Task<Device> GetDevice(string id);

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        Task<Device> UpdateDevice(Device device);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration.</returns>
        Task<IManagerConfiguration> GetManagerConfiguration();

        /// <summary>
        /// Gets the appium processes.
        /// </summary>
        /// <returns>The appium processes.</returns>
        Task<IEnumerable<AppiumProcess>> GetAppiumProcesses();
    }
}
