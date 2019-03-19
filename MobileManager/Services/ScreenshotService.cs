using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Controllers;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MobileManager.Services
{
    /// <inheritdoc>
    ///     <cref>IScreenshotService</cref>
    /// </inheritdoc>
    /// <summary>
    /// Screenshot service.
    /// </summary>
    public class ScreenshotService : ControllerExtensions, IScreenshotService
    {
        private readonly IManagerLogger _logger;
        private readonly IExternalProcesses _externalProcesses;


        /// <inheritdoc />
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="externalProcesses"></param>
        public ScreenshotService(IManagerLogger logger, IExternalProcesses externalProcesses) : base(logger)
        {
            _logger = logger;
            _externalProcesses = externalProcesses;
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes the screenshot ios device.
        /// </summary>
        /// <returns>The screenshot ios device.</returns>
        /// <param name="device">Device.</param>
        public FileStreamResult TakeScreenshotIosDevice(Device device)
        {
            var screenshotFolder = Path.Combine(Directory.GetCurrentDirectory(), "Screenshot");
            Directory.CreateDirectory(screenshotFolder);
            var screenshotFilePath = Path.Combine(screenshotFolder, $"{device.Id}.tiff");

            var screenshotRet = _externalProcesses.RunProcessAndReadOutput("idevicescreenshot",
                $" -u {device.Id} {screenshotFilePath}", 10000);
            _logger.Debug(screenshotRet);

            if (screenshotRet.Contains("Could not start screenshotr service"))
            {
                //throw new Exception($"Failed get screenshot. {screenshotRet}");
                return GetDefaultMobileImage();
            }

            var convertedImagePath = Path.Combine(screenshotFolder, $"{device.Id}.jpg");
            ConvertImage(screenshotFilePath, convertedImagePath);

            FileStream image;

            if (System.IO.File.Exists(convertedImagePath))
            {
                image = System.IO.File.OpenRead(convertedImagePath);
                return File(image, "image/jpeg");
            }

            image = System.IO.File.OpenRead(screenshotFilePath);
            return File(image, "image/tiff");
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes the screenshot android device.
        /// </summary>
        /// <returns>The screenshot android device.</returns>
        /// <param name="device">Device.</param>
        public FileStreamResult TakeScreenshotAndroidDevice(Device device)
        {
            try
            {
                var screenshotFolder = Path.Combine(Directory.GetCurrentDirectory(), "Screenshot");
                Directory.CreateDirectory(screenshotFolder);
                var screenshotFilePath = Path.Combine(screenshotFolder, $"{device.Id}.png");

                // adb shell screencap -p | perl -pe 's/\x0D\x0A/\x0A/g' > screen.png
                var screenshotRet = _externalProcesses.RunShellProcess("adb",
                    $" -s {device.Id} exec-out 'screencap -p' > {screenshotFilePath}; exit 0", 10000);
                _logger.Debug(screenshotRet);

                if (screenshotRet.Contains("error:"))
                {
                    return GetDefaultMobileImage();
                }

                if (new FileInfo(screenshotFilePath).Length == 0)
                {
                    _logger.Error(
                        $"Failed to get screenshot for device: [{device.Id}]. Screenshot file [{screenshotFilePath}] has 0 size.");
                    return GetDefaultMobileImage();
                }

                var convertedImagePath = Path.Combine(screenshotFolder, $"{device.Id}.jpg");

                ConvertImage(screenshotFilePath, convertedImagePath);

                FileStream image;

                if (System.IO.File.Exists(convertedImagePath))
                {
                    image = System.IO.File.OpenRead(convertedImagePath);
                    return File(image, "image/jpeg");
                }

                image = System.IO.File.OpenRead(screenshotFilePath);
                return File(image, "image/png");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to get screenshot for device: [{device.Id}].", e);
                return GetDefaultMobileImage();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Loads the screenshot for offline device.
        /// </summary>
        /// <returns>The screenshot for offline device.</returns>
        /// <param name="device">Device.</param>
        public FileStreamResult LoadScreenshotForOfflineDevice(Device device)
        {
            return GetDefaultMobileImage();
        }

        private FileStreamResult GetDefaultMobileImage()
        {
            return File(System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "phone_default.png")),
                "image/png");
        }

        private void ConvertImage(string screenshotFilePath, string convertedImagePath)
        {
            try
            {
                using (var imageToConvert = Image.Load(screenshotFilePath))
                {
                    imageToConvert.Mutate(x => x.Resize(imageToConvert.Width / 2, imageToConvert.Height / 2));
                    var i = 0;
                    if (System.IO.File.Exists(convertedImagePath))
                    {
                        while (IsFileLocked(new FileInfo(convertedImagePath)))
                        {
                            if (i > 200)
                            {
                                throw new TimeoutException(
                                    $"ConvertImage: Failed to get access to file {convertedImagePath}");
                            }

                            _logger.Debug($"Screenshot file is locked[{++i}]: {convertedImagePath}.");
                            Thread.Sleep(100);
                        }
                    }

                    imageToConvert.Save(convertedImagePath);
                    // automatic encoder selected based on extension.
                }
            }
            catch (Exception e)
            {
                _logger.Error($"ConvertImage caught exception.", e);
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Dispose();
            }

            //file is not locked
            return false;
        }
    }
}
