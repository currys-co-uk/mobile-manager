using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MobileManager.Logging.Logger
{
    /// <summary>
    /// Logger.
    /// </summary>
    public class ManagerLogger : IManagerLogger
    {
        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>The log.</value>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Logging.Logger.Logger"/> class.
        /// </summary>
        public ManagerLogger()
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            loggerFactory.AddFile("Logs/log-{Date}.txt", LogLevel.Trace);
            _logger = loggerFactory.CreateLogger("MobileManager");
        }

        /// <summary>
        /// Info the specified message.
        /// </summary>
        /// <returns>The info.</returns>
        /// <param name="message">Message.</param>
        /// <param name="callerName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public void Info(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogInformation($"{DateTime.Now}: [INFO]: {message}");
        }

        /// <summary>
        /// Error the specified message.
        /// </summary>
        /// <returns>The error.</returns>
        /// <param name="message">Message.</param>
        /// <param name="callerName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public void Error(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogError($"{DateTime.Now}: [ERROR]: {message}");
        }

        /// <summary>
        /// Error the specified message and exception
        /// </summary>
        /// <returns>The error.</returns>
        /// <param name="message">Message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="callerName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public void Error(string message, Exception e,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogError($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: [ERROR]: {e.Message}");
            _logger.LogError($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: {e.StackTrace}");
        }

        /// <summary>
        /// Debug the specified message.
        /// </summary>
        /// <returns>The debug.</returns>
        /// <param name="message">Message.</param>
        /// <param name="callerName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public void Debug(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogDebug($"{DateTime.Now}: [DEBUG]: {message}");
        }
    }
}
