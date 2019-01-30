using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Adb;
using MobileManager.Models.Devices.Enums;
using MobileManager.Services;

namespace MobileManager.Controllers
{
    /// <inheritdoc cref="IAdbController" />
    /// <summary>
    /// Adb controller.
    /// </summary>
    [Route("api/v1/adb")]
    [EnableCors("AllowAllHeaders")]
    public class AdbController : ControllerExtensions, IAdbController
    {
        private readonly IRestClient _restClient;
        private readonly IManagerLogger _logger;
        private readonly IExternalProcesses _externalProcesses;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Controllers.AdbController" /> class.
        /// </summary>
        /// <param name="restClient">Rest client.</param>
        /// <param name="logger">Logger</param>
        /// <param name="externalProcesses"></param>
        public AdbController(IRestClient restClient, IManagerLogger logger, IExternalProcesses externalProcesses) : base(logger)
        {
            _restClient = restClient;
            _logger = logger;
            _externalProcesses = externalProcesses;
        }

        /// <inheritdoc />
        /// <summary>
        /// Executes ADB command via local ADB
        /// </summary>
        /// <remarks>
        /// Full list of ADB commands can be found here: https://developer.android.com/studio/command-line/adb.html
        /// </remarks>
        /// <returns>Result of ADB command</returns>
        /// <param name="adbCommand">ADB command without "adb" executable in the name</param>
        /// <response code="200">Command executed successfully.</response>
        /// <response code="500">Failed to run adb command.</response>
        [HttpPost("command")]
        public IActionResult Command([FromBody] AdbCommand adbCommand)
        {
            if (!IsAdbCommandExecutable(adbCommand, out var actionResult)) return actionResult;

            string output;
            try
            {
                output = _externalProcesses.RunProcessAndReadOutput("adb",
                    $"-s {adbCommand.AndroidDeviceId} {adbCommand.Command}");
                _logger.Debug($"{nameof(AdbCommand)} [{adbCommand.Command}] output: [{output}]");
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to run adb command. " + ex.Message);
            }

            return StatusCodeExtension(200, output);
        }

        /// <inheritdoc />
        /// <summary>
        /// Executes ADB shell command via local ADB
        /// </summary>
        /// <remarks>
        /// Full list of ADB commands can be found here: https://developer.android.com/studio/command-line/adb.html
        /// </remarks>
        /// <returns>Result of ADB command</returns>
        /// <param name="adbCommand">ADB shell command without "adb shell" executable in the name</param>
        /// <response code="200">Command executed successfully.</response>
        /// <response code="500">Failed to run adb command.</response>
        [HttpPost("shellCommand")]
        public IActionResult ShellAdbCommand([FromBody] AdbCommand adbCommand)
        {
            if (!IsAdbCommandExecutable(adbCommand, out var actionResult)) return actionResult;

            string output;
            try
            {
                output = _externalProcesses.RunProcessAndReadOutput("adb",
                    $"-s {adbCommand.AndroidDeviceId} shell {adbCommand.Command}");
                _logger.Debug($"{nameof(AdbCommand)} shell [{adbCommand.Command}] output: [{output}]");
            }
            catch (Exception ex)
            {
                return StatusCodeExtension(500, "Failed to run adb command. " + ex.Message);
            }

            return StatusCodeExtension(200, output);
        }

        private bool IsAdbCommandExecutable(IAdbCommand adbCommand, out IActionResult actionResult)
        {
            actionResult = null;

            LogRequestToDebug();

            if (adbCommand?.AndroidDeviceId == null || adbCommand.Command == null)
            {
                {
                    actionResult = BadRequestExtension("Empty AdbCommand in request");
                    return false;
                }
            }

            var device = _restClient.GetDevice(adbCommand.AndroidDeviceId).Result;
            if (device == null)
            {
                {
                    actionResult = BadRequestExtension(
                        $"Device with DeviceId [{adbCommand.AndroidDeviceId}] not found in device pool.");
                    return false;
                }
            }

            if (device.Type != DeviceType.Android)
            {
                {
                    actionResult = BadRequestExtension(
                        $"Device found by ID is not an Android. [{device}]");
                    return false;
                }
            }

            return true;
        }
    }
}
