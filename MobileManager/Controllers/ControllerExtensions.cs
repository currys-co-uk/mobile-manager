using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Logging.Logger;
using Newtonsoft.Json;

namespace MobileManager.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controller extensions.
    /// </summary>
    public class ControllerExtensions : Controller
    {
        private readonly IManagerLogger _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ControllerExtensions(IManagerLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends bad request extension.
        /// </summary>
        /// <returns>The request extension.</returns>
        /// <param name="error">Error.</param>
        [NonAction]
        public BadRequestObjectResult BadRequestExtension(object error)
        {
            _logger.Debug(string.Format("Response status code: [{0}], message: [{1}]", 400,
                JsonConvert.SerializeObject(error)));
            return BadRequest(error);
        }

        /// <summary>
        /// Statuses the code extension.
        /// </summary>
        /// <returns>The code extension.</returns>
        /// <param name="statusCode">Status code.</param>
        /// <param name="value">Value.</param>
        [NonAction]
        public ObjectResult StatusCodeExtension(int statusCode, object value)
        {
            _logger.Debug(string.Format("Response status code: [{0}], message: [{1}]", statusCode,
                JsonConvert.SerializeObject(value)));
            return StatusCode(statusCode, value);
        }

        /// <summary>
        /// Send json extension.
        /// </summary>
        /// <returns>The extension.</returns>
        /// <param name="data">Data.</param>
        [NonAction]
        public JsonResult JsonExtension(object data)
        {
            _logger.Debug(string.Format("Json data response: [{0}]", JsonConvert.SerializeObject(data)));
            return Json(data);
        }

        /// <summary>
        /// Send the ok extension.
        /// </summary>
        /// <returns>The extension.</returns>
        /// <param name="message">Message.</param>
        [NonAction]
        public OkResult OkExtension(string message = "")
        {
            _logger.Debug(string.Format("OK Response status code: [{0}] with message: [{1}]", 200, message));
            return Ok();
        }

        /// <summary>
        /// Objects the result extension.
        /// </summary>
        /// <returns>The result extension.</returns>
        /// <param name="data">Data.</param>
        [NonAction]
        public ObjectResult ObjectResultExtension(object data)
        {
            _logger.Debug(string.Format("Json data response: [{0}]", JsonConvert.SerializeObject(data)));
            return new ObjectResult(data);
        }

        /// <summary>
        /// Sends not found extension.
        /// </summary>
        /// <returns>The found extension.</returns>
        /// <param name="data">Data.</param>
        [NonAction]
        public ObjectResult NotFoundExtension(object data)
        {
            _logger.Debug(string.Format("NotFound response: [{0}]", JsonConvert.SerializeObject(data)));
            return NotFound(data);
        }

        /// <summary>
        /// Log incoming request data to debug.
        /// </summary>
        [NonAction]
        public void LogRequestToDebug()
        {
            _logger.Debug($"{GetDetails(Request)}");
        }

        private static string GetDetails(HttpRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString.Value}";
            var sbHeaders = new StringBuilder();
            foreach (var header in request.Headers)
                sbHeaders.Append($"{header.Key}: {header.Value}\n");

            var body = "no-body";
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(request.Body))
                    body = sr.ReadToEnd();
            }

            return
                $"Protocol: {request.Protocol}, Method: {request.Method}, Url: {baseUrl}, Headers: {sbHeaders}, Body: {body}";
        }
    }
}
