using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Logger;

namespace MobileManager.Controllers
{
    /// <summary>
    /// MM log controller.
    /// </summary>
    [Route("api/v1/log/")]
    [EnableCors("AllowAllHeaders")]
    public class MMLogController : ControllerExtensions, IMmLogController
    {
        private readonly IRepository<LogMessage> _repository;
        private readonly IManagerLogger _logger;

        /// <inheritdoc />
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">Logger repository.</param>
        /// <param name="logger">Logger.</param>
        public MMLogController(IRepository<LogMessage> repository, IManagerLogger logger) : base(logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets last number of lines from MM log
        /// </summary>
        /// <returns>MM log.</returns>
        /// <param name="numberOfLines">Specify number of last lines to display</param>
        /// <param name="filter">string filter for fulltext search</param>
        /// <response code="200">MM log returned successfully.</response>
        /// <response code="404">MM log or device id not found.</response>
        [HttpGet("filter", Name = "getMMLog")]
        public IActionResult GetLines(int numberOfLines, string filter = "")
        {
            LogRequestToDebug();

            var logMessages = _repository.GetAll().Where(l => l.Message.Contains(filter)).Take(numberOfLines);

            return JsonExtension(logMessages);
        }
    }
}
