using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Controllers.Interfaces;
using MobileManager.Logging.Logger;

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IManagerConfigurationController" />
    /// <summary>
    /// Configuration controller.
    /// </summary>
    [Route("api/v1/configuration")]
    [EnableCors("AllowAllHeaders")]
    public class ConfigurationController : ControllerExtensions, IManagerConfigurationController
    {
        private readonly IManagerLogger _logger;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.ConfigurationController" /> class.
        /// </summary>
        public ConfigurationController(IManagerLogger logger) : base(logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets internal configuration
        /// </summary>
        /// <returns>Internal configuration</returns>
        [HttpGet]
        public IActionResult Get()
        {
            LogRequestToDebug();

            return JsonExtension(AppConfigurationProvider.Get<ManagerConfiguration>());
        }

        /// <inheritdoc />
        /// <summary>
        /// Update the internal configuration.
        /// </summary>
        /// <returns>Updated internal configuration.</returns>
        /// <param name="configuration">Configuration.</param>
        /// <response code="200">Configuration returned successfully.</response>
        /// <response code="400">Empty request.</response>
        [HttpPut]
        public IActionResult Update([FromBody] ManagerConfiguration configuration)
        {
            LogRequestToDebug();
            return StatusCodeExtension(501, "This method is temporarily disabled.");
        }
    }
}
