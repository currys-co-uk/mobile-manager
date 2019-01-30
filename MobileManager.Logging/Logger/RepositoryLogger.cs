using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.Logger;
using Newtonsoft.Json;

namespace MobileManager.Logging.Logger
{
    /// <inheritdoc cref="IManagerLogger" />
    /// <summary>
    /// Logs messages to repository.
    /// </summary>
    public class RepositoryLogger : IManagerLogger, IDisposable, IHostedService
    {
        private readonly IRepository<LogMessage> _repository;

        private readonly ManagerLogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly List<LogMessage> _logMessageStore;
        private Task _repositoryLoggerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Logging.Logger.Logger"/> class.
        /// </summary>
        public RepositoryLogger(IRepository<LogMessage> repository)
        {
            _repository = repository;
            _logger = new ManagerLogger();
            _logMessageStore = new List<LogMessage>();
            _cancellationTokenSource = new CancellationTokenSource();

            StartAsync(_cancellationTokenSource.Token);
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
            _logger.Info(message);
            AddToRepository(LogLevel.Information, $"{Path.GetFileName(fileName)}:{lineNumber}.{callerName}", message);
        }

        /// <summary>
        /// Error the specified message.
        /// </summary>
        /// <returns>The error.</returns>
        /// <param name="message">Message.</param>
        /// <param name="callerName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public void Error(String message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.Error(message);
            AddToRepository(LogLevel.Error, $"{Path.GetFileName(fileName)}:{lineNumber}.{callerName}", message);
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
        public void Error(string message,
            Exception e,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger.Error(message, e);
            AddToRepository(LogLevel.Error, $"{Path.GetFileName(fileName)}:{lineNumber}.{callerName}", message, e);
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
            _logger.Debug(message);
            AddToRepository(LogLevel.Debug, $"{Path.GetFileName(fileName)}:{lineNumber}.{callerName}", message);
        }

        private void AddToRepository(LogLevel logLevel, string callerMethod, string message, Exception e = null)
        {
            var logMessage = new LogMessage
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                Message = message,
                Exception = e?.StackTrace,
                MethodName = callerMethod,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            _logMessageStore.Add(logMessage);
        }

        private async Task StoreMessagesToRepository(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_logMessageStore.Any())
                {
                    var logMessage = _logMessageStore.First();
                    try
                    {
                        _repository.Add(logMessage);
                        _logMessageStore.Remove(logMessage);
                    }
                    catch (Exception e)
                    {
                        Error($"Failed to store message [{JsonConvert.SerializeObject(logMessage)}] in database.", e);
                        await Task.Delay(5000, cancellationToken);
                    }
                }
                else
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _repositoryLoggerService.Wait(_cancellationTokenSource.Token);
            _repositoryLoggerService?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _repositoryLoggerService =
                Task.Factory.StartNew(async () => { await StoreMessagesToRepository(cancellationToken); },
                    cancellationToken);
            return _repositoryLoggerService;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _repository.Add(_logMessageStore);
            _repositoryLoggerService.Wait(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
