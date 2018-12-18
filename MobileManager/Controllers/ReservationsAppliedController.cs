using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Reservations;
using MobileManager.Services.Interfaces;
using MobileManager.Utils;
using Newtonsoft.Json;

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IReservationsAppliedController" />
    /// <summary>
    /// Reservations applied controller.
    /// </summary>
    [Route("api/v1/reservation/applied")]
    [EnableCors("AllowAllHeaders")]
    public class ReservationsAppliedController : ControllerExtensions, IReservationsAppliedController
    {
        private readonly IRepository<ReservationApplied> _reservationsAppliedRepository;
        private readonly IRestClient _restClient;
        private readonly IAppiumService _appiumService;
        private readonly IManagerLogger _logger;
        private readonly DeviceUtils _deviceUtils;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.ReservationsAppliedController" /> class.
        /// </summary>
        /// <param name="reservationsAppliedRepository">Reservations applied repository.</param>
        /// <param name="restClient">Rest client.</param>
        /// <param name="appiumService">Appium service.</param>
        /// <param name="logger">Logger.</param>
        public ReservationsAppliedController(IRepository<ReservationApplied> reservationsAppliedRepository,
            IRestClient restClient, IAppiumService appiumService, IManagerLogger logger) : base(logger)
        {
            _reservationsAppliedRepository = reservationsAppliedRepository;
            _restClient = restClient;
            _appiumService = appiumService;
            _logger = logger;
            _deviceUtils = new DeviceUtils(_logger);
        }

        /// <summary>
        /// Gets all applied reservations.
        /// </summary>
        /// <returns>The all applied reservations.</returns>
        [HttpGet]
        public IEnumerable<ReservationApplied> GetAllAppliedReservations()
        {
            LogRequestToDebug();

            var reservations = _reservationsAppliedRepository.GetAll();
            _logger.Debug(
                string.Format("GetAll reservations applied: [{0}]", JsonConvert.SerializeObject(reservations)));
            return reservations;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the reservationApplied by identifier.
        /// </summary>
        /// <returns>ReservationApplied.</returns>
        /// <param name="id">ReservationApplied Identifier.</param>
        [HttpGet("{id}", Name = "getAppliedReservation")]
        public IActionResult GetById(string id)
        {
            LogRequestToDebug();

            var reservationFromApplied = _reservationsAppliedRepository.Find(id);

            if (reservationFromApplied == null)
            {
                return NotFoundExtension("Reservation not found in the database.");
            }

            return JsonExtension(reservationFromApplied);
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates the specified reservation. [INTERNAL-ONLY]
        /// </summary>
        /// <returns>Created reservationApplied.</returns>
        /// <param name="reservation">ReservationApplied.</param>
        /// <response code="200">ReservationApplied created successfully.</response>
        /// <response code="400">Invalid reservationApplied in request</response>
        /// <response code="500">Internal failure.</response>
        [HttpPost]
        public IActionResult Create([FromBody] ReservationApplied reservation)
        {
            LogRequestToDebug();

            if (reservation == null)
            {
                return BadRequestExtension("Reservation is empty.");
            }

            if (reservation.ReservedDevices == null || !reservation.ReservedDevices.Any())
            {
                return BadRequestExtension("RequestedDevices property is empty.");
            }

            try
            {
                _reservationsAppliedRepository.Add(reservation);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to Add reservation in database. " + ex.Message);
            }

            _logger.Debug(string.Format("Created new applied reservation: [{0}]",
                JsonConvert.SerializeObject(reservation)));

            return CreatedAtRoute("getAppliedReservation", new {id = reservation.Id}, reservation);
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the ReservationApplied.
        /// </summary>
        /// <returns>null.</returns>
        /// <param name="id">ReservationApplied Identifier.</param>
        /// <response code="200">ReservationApplied deleted successfully.</response>
        /// <response code="400">Invalid id in request</response>
        /// <response code="500">Internal failure.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            LogRequestToDebug();

            var reservationFromApplied = _reservationsAppliedRepository.Find(id);

            if (reservationFromApplied == null)
            {
                return NotFoundExtension("Reservation not found in database.");
            }

            foreach (var reservedDevice in reservationFromApplied.ReservedDevices)
            {
                try
                {
                    //todo: change to exception handling when UnlockDevice is developed
                    if (!(await _deviceUtils.UnlockDevice(reservedDevice.DeviceId, _restClient, _appiumService))
                        .Available)
                    {
                        return StatusCodeExtension(500,
                            "Failed to unlock device id: " + reservedDevice.DeviceId + " from reservation.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to remove reserved devices due to: {ex.Message}.", ex);
                    return StatusCodeExtension(500,
                        "Failed to unlock device id: " + reservedDevice.DeviceId + " from reservation.");
                }
            }


            try
            {
                _reservationsAppliedRepository.Remove(id);
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to Remove reservation from database. " + ex.Message);
            }

            return OkExtension(string.Format("Reservation queued successfully deleted: [{0}]",
                JsonConvert.SerializeObject(reservationFromApplied)));
        }
    }
}
