using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IAdminController" />
    /// <summary>
    /// Admin controller
    /// </summary>
    [Route("api/v1/admin")]
    [EnableCors("AllowAllHeaders")]
    public class AdminController : ControllerExtensions, IAdminController
    {
        private readonly IRestClient _restClient;
        private readonly IManagerLogger _logger;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.AdminController" /> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="logger">Logger</param>
        public AdminController(IRestClient restClient, IManagerLogger logger) : base(logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all repositories.
        /// </summary>
        /// <returns>The all repositories.</returns>
        [HttpGet("repositories", Name = "getAllRepositories")]
        public async Task<IActionResult> GetAllRepositoriesAsync()
        {
            LogRequestToDebug();

            var resultDevices = await _restClient.GetDevices();
            var resultReservationQueue = await _restClient.GetReservations();
            var resultReservationApplied = await _restClient.GetAppliedReservations();
            var resultConfiguration = await _restClient.GetManagerConfiguration();
            var resultAppiumProcesses = await _restClient.GetAppiumProcesses();

            var result = new
            {
                Devices = resultDevices,
                Reservations = resultReservationQueue,
                ReservationsApplied = resultReservationApplied,
                Configuration = resultConfiguration,
                AppiumProcesses = resultAppiumProcesses
            };

            return JsonExtension(result);
        }
    }
}
