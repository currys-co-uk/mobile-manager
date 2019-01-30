using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Interfaces;
using MobileManager.Models.Reservations;
using MobileManager.Services.Interfaces;
using MobileManager.Utils;
using Newtonsoft.Json;

namespace MobileManager.Services
{
    /// <inheritdoc cref="IReservationService" />
    /// <summary>
    /// Reservation service.
    /// </summary>
    public class ReservationService : IReservationService, IHostedService, IDisposable
    {
        private readonly IManagerLogger _logger;
        private RestClient RestClient { get; }
        private readonly AppiumService _appiumService;
        private Task _reservationService;
        private readonly DeviceUtils _deviceUtils;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Services.ReservationService"/> class.
        /// </summary>
        public ReservationService(IManagerConfiguration configuration, IManagerLogger logger, IExternalProcesses externalProcesses)
        {
            _logger = logger;
            var externalProcesses1 = externalProcesses;
            _deviceUtils = new DeviceUtils(_logger, externalProcesses1);
            RestClient = new RestClient(configuration, _logger);
            _appiumService = new AppiumService(configuration, logger, externalProcesses1);
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _reservationService =
                Task.Factory.StartNew(async () => { await RunAsyncApplyReservationTaskAsync(cancellationToken); },
                    cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _reservationService.Wait(cancellationToken);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reservationService?.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// Runs the async apply reservation task async.
        /// </summary>
        public async Task RunAsyncApplyReservationTaskAsync(CancellationToken cancellationToken)
        {
            _logger.Info("RunAsyncApplyReservationTaskAsync Thread Started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var applied = false;
                    if (await RestClient.TryToConnect())
                    {
                        _logger.Info("ApplyAvailableReservations [START]");
                        try
                        {
                            applied = await ApplyAvailableReservations();
                        }
                        catch (Exception e)
                        {
                            _logger.Info("ApplyAvailableReservations: " + e.Message + " [ERROR]");
                        }

                        _logger.Info("ApplyAvailableReservations: " + applied + " [STOP]");

                        Thread.Sleep((await RestClient.GetManagerConfiguration()).ReservationServiceRefreshTime);
                    }
                    else
                    {
                        _logger.Error("ApplyAvailableReservations: Failed connecting to " + RestClient.Endpoint +
                                      " [STOP]");
                        var sleep = AppConfigurationProvider.Get<ManagerConfiguration>().GlobalReconnectTimeout;
                        _logger.Info("ApplyAvailableReservations Sleep for [ms]: " + sleep);
                        Thread.Sleep(sleep);
                        _logger.Info("ApplyAvailableReservations Sleep finished");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Exception during RunAsyncApplyReservationTaskAsync.", e);
                }
            }

            _logger.Info($"{nameof(RunAsyncApplyReservationTaskAsync)} STOP.");
        }

        /// <inheritdoc />
        /// <summary>
        /// Applies the available reservations.
        /// </summary>
        /// <returns>The available reservations.</returns>
        //todo refactor - split into multiple methods
        public async Task<bool> ApplyAvailableReservations()
        {
            var reservationQueue = await LoadReservationQueue();
            var appliedReservations = new List<ReservationApplied>();
            foreach (var reservation in reservationQueue)
            {
                _logger.Info($"Applying reservation - item in queue: {JsonConvert.SerializeObject(reservation)}");
                var reservedDevices = new List<ReservedDevice>();

                if (reservation.RequestedDevices == null)
                {
                    throw new NullReferenceException("RequestedDevices property on reservation is empty.");
                }

                var reservationEligible = true;
                if (reservation.RequestedDevices.Count > 1)
                {
                    reservationEligible = await IsReservationEligible(reservation);
                }

                if (reservationEligible)
                {
                    await ReserveAllRequestedDevices(reservation, reservedDevices);
                }
                else
                {
                    _logger.Debug($"Reservation is not eligible. {JsonConvert.SerializeObject(reservation)}");
                    continue;
                }

                if (reservedDevices.Count == reservation.RequestedDevices.Count)
                {
                    //ICollection<ReservationApplied> reservationApplied = null;
                    try
                    {
                        var reservationApplied = await AddAppliedReservation(appliedReservations, reservation,
                            reservedDevices);
                        appliedReservations.AddRange(reservationApplied);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to AddAppliedReservation.", e);
                        await UnlockAllReservedDevices(reservedDevices);
                        continue;
                    }

                    _logger.Info(
                        $"Applying reservation - removing reservation from queue: {JsonConvert.SerializeObject(reservation)}");
                    await RestClient.DeleteReservation(reservation.Id);
                }
                else if (reservedDevices.Any())
                {
                    await UnlockAllReservedDevices(reservedDevices);
                }
            }

            _logger.Info(
                $"Applying reservation - applied reservations: {JsonConvert.SerializeObject(appliedReservations)}");
            return appliedReservations.Any();
        }

        /// <inheritdoc />
        /// <summary>
        /// Loads the reservation queue.
        /// </summary>
        /// <returns>The reservation queue.</returns>
        public async Task<IEnumerable<Reservation>> LoadReservationQueue()
        {
            return await RestClient.GetReservations();
        }

        #region privateMethods

        private async Task UnlockAllReservedDevices(List<ReservedDevice> reservedDevices)
        {
            _logger.Error(
                $"Applying reservation - failed to lock all requested devices: {JsonConvert.SerializeObject(reservedDevices)}");
            foreach (var reserveDevice in reservedDevices)
            {
                _logger.Debug($"Applying reservation - unlock device: {JsonConvert.SerializeObject(reserveDevice)}");
                if (!(await _deviceUtils.UnlockDevice(reserveDevice.DeviceId, RestClient, _appiumService)).Available)
                {
                    throw new Exception(
                        "Applying reservation failed to lock all. ReservationService failed to unlock device " +
                        reserveDevice.DeviceId);
                }
            }
        }

        private async Task<ICollection<ReservationApplied>> AddAppliedReservation(
            ICollection<ReservationApplied> appliedReservations, Reservation reservation,
            List<ReservedDevice> reservedDevices)
        {
            _logger.Debug(
                $"Applying reservation - all devices are locked: {JsonConvert.SerializeObject(reservedDevices)}");
            reservation.Available = true;
            var reservationToBeApplied = new ReservationApplied(reservation.Id)
            {
                ReservedDevices = reservedDevices,
                Available = true
            };

            _logger.Debug(
                $"Applying reservation - adding reservation applied: {JsonConvert.SerializeObject(reservationToBeApplied)}");
            var appliedReservation = await RestClient.ApplyReservation(reservationToBeApplied);
            appliedReservations.Add(appliedReservation);

            return appliedReservations;
        }

        private async Task ReserveAllRequestedDevices(Reservation reservation, List<ReservedDevice> reservedDevices)
        {
            foreach (var requestedDevice in reservation.RequestedDevices)
            {
                //var device = await RestClient.GetDevice(requestedDevice.DeviceId);

                var device = await _deviceUtils.FindMatchingDevice(requestedDevice, RestClient);

                if (device == null)
                {
                    _logger.Error("Device " + JsonConvert.SerializeObject(requestedDevice) + " not found.");
                    continue;
                }

                _logger.Debug($"Applying reservation - device is found: {JsonConvert.SerializeObject(device)}");

                if (!reservation.Available && await IsDeviceAvailable(device))
                {
                    _logger.Debug($"Applying reservation - locking device: {JsonConvert.SerializeObject(device)}");
                    try
                    {
                        var updatedDevice = await _deviceUtils.LockDevice(device.Id, RestClient, _appiumService);
                        if (!updatedDevice.Available)
                        {
                            _logger.Debug(
                                $"Applying reservation - failed to lock device: {JsonConvert.SerializeObject(updatedDevice)}");
                            ReservedDevice reservedDevice = new ReservedDevice(updatedDevice);
                            reservedDevices.Add(reservedDevice);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to apply reservation {reservation.Id}", e);
                        reservation.FailedToApply++;
                        await RestClient.UpdateReservation(reservation);
                    }
                }
            }
        }

        private async Task<bool> IsReservationEligible(Reservation reservation)
        {
            var reservationAligable = true;
            _logger.Debug(
                $"Applying reservation - contains multiple devices: {JsonConvert.SerializeObject(reservation.RequestedDevices)}");

            var listOfLockedDevices = new List<Device>();

            foreach (var requestedDevice in reservation.RequestedDevices)
            {
                var device = await _deviceUtils.FindMatchingDevice(requestedDevice, RestClient);

                if (device == null)
                {
                    _logger.Error("Device " + JsonConvert.SerializeObject(requestedDevice) + " not found.");
                    reservationAligable = false;
                    continue;
                }

                if (!device.Available)
                {
                    _logger.Debug(
                        $"Applying reservation - device is not available: {device.Id}");
                    reservationAligable = false;
                }
                else
                {
                    device.Available = false;
                    listOfLockedDevices.Add(await RestClient.UpdateDevice(device));
                    _logger.Debug($"Applying reservation - device is available: {device.Id}");
                }
            }

            if (!reservationAligable)
            {
                var tasks = new List<Task>();

                foreach (var lockedDevice in listOfLockedDevices)
                {
                    lockedDevice.Available = true;
                    tasks.Add(RestClient.UpdateDevice(lockedDevice));
                }

                await Task.WhenAll(tasks);
            }

            return reservationAligable;
        }

        private async Task<bool> IsDeviceAvailable(IDevice device)
        {
            var dev = await RestClient.GetDevice(device.Id);
            return dev.Available;
        }

        #endregion
    }
}
