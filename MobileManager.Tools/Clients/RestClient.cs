using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MobileManager.Appium;
using MobileManager.Configuration;
using MobileManager.Configuration.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Reservations;
using Newtonsoft.Json;

namespace MobileManager.Http.Clients
{
    /// <inheritdoc />
    /// <summary>
    /// Rest client.
    /// </summary>
    public class RestClient : IRestClient
    {
        private readonly IManagerLogger _logger;
        private readonly HttpClient _client;

        public string Endpoint { get; }

        public RestClient(IManagerConfiguration configuration, IManagerLogger logger)
        {
            _logger = logger;
            Endpoint = $"http://{configuration.LocalIpAddress}:{configuration.ListeningPort}";
            _client = new HttpClient
            {
                BaseAddress = new Uri(Endpoint),
                Timeout = new TimeSpan(0, 0, 0, 60)
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Http.Clients.RestClient"/> class. For unit tests only.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="client">Client.</param>
        /// <param name="logger">Logger.</param>
        public RestClient(IManagerConfiguration configuration, HttpClient client, IManagerLogger logger)
        {
            Endpoint = $"http://{configuration.LocalIpAddress}:{configuration.ListeningPort}";

            _client = client;
            _logger = logger;
            _client.BaseAddress = new Uri(Endpoint);
            _client.Timeout = new TimeSpan(0, 0, 0, 10);
        }

        /// <inheritdoc />
        /// <summary>
        /// Tries to connect.
        /// </summary>
        /// <returns>The to connect.</returns>
        public async Task<bool> TryToConnect()
        {
            _logger.Debug("Starting TryToConnect");
            try
            {
                await _client.GetAsync("api/v1/configuration");
            }
            catch (Exception e)
            {
                _logger.Info("Failed to connect to: " + _client.BaseAddress);
                _logger.Error("Failed to connect.", e);
                return false;
            }

            _logger.Debug("Finished TryToConnect OK");
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        public async Task<Device> AddDevice(Device device)
        {
            var serializedDevice = JsonConvert.SerializeObject(device);
            _logger.Debug(serializedDevice);
            var httpContent = new StringContent(serializedDevice, Encoding.UTF8, "application/json");

            _logger.Debug("AddDevice: " + _client.BaseAddress + "api/v1/device");
            var response = await _client.PostAsync("api/v1/device", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return await GetDevice(device.Id);
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>The devices.</returns>
        public async Task<IEnumerable<Device>> GetDevices()
        {
            IEnumerable<Device> devices = new List<Device>();

            var response = await _client.GetAsync("api/v1/device");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                devices = JsonConvert.DeserializeObject<IEnumerable<Device>>(stream);
            }

            return devices;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the reservations.
        /// </summary>
        /// <returns>The reservations.</returns>
        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            IEnumerable<Reservation> reservations = new List<Reservation>();

            var response = await _client.GetAsync("api/v1/reservation");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                reservations = JsonConvert.DeserializeObject<IEnumerable<Reservation>>(stream);
            }

            return reservations;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the applied reservations.
        /// </summary>
        /// <returns>The applied reservations.</returns>
        public async Task<IEnumerable<ReservationApplied>> GetAppliedReservations()
        {
            IEnumerable<ReservationApplied> reservations = new List<ReservationApplied>();

            var response = await _client.GetAsync("api/v1/reservation/applied");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                reservations = JsonConvert.DeserializeObject<IEnumerable<ReservationApplied>>(stream);
            }

            return reservations;
        }

        /// <inheritdoc />
        /// <summary>
        /// Applies the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservation">Reservation.</param>
        public async Task<ReservationApplied> ApplyReservation(ReservationApplied reservation)
        {
            var serializedReservation = JsonConvert.SerializeObject(reservation);
            var httpContent = new StringContent(serializedReservation, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/v1/reservation/applied", httpContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(response.ToString());
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return await GetAppliedReservation(reservation.Id);
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservation">Reservation.</param>
        public async Task<Reservation> UpdateReservation(Reservation reservation)
        {
            var serializedReservation = JsonConvert.SerializeObject(reservation);
            var httpContent = new StringContent(serializedReservation, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync("api/v1/reservation/" + reservation.Id, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return await GetReservation(reservation.Id);
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="reservationId">Reservation identifier.</param>
        public async Task<bool> DeleteReservation(string reservationId)
        {
            var response = await _client.DeleteAsync("api/v1/reservation/" + reservationId);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the applied reservation.
        /// </summary>
        /// <returns>The applied reservation.</returns>
        /// <param name="reservationAppliedId">Reservation applied identifier.</param>
        public async Task<bool> DeleteAppliedReservation(string reservationAppliedId)
        {
            var response = await _client.DeleteAsync("api/v1/reservation/applied/" + reservationAppliedId);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the reservation.
        /// </summary>
        /// <returns>The reservation.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<Reservation> GetReservation(string id)
        {
            Reservation reservation = null;

            _logger.Debug("GetReservation: " + _client.BaseAddress + "api/v1/reservation/" + id);
            var response = await _client.GetAsync("api/v1/reservation/" + id);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                reservation = JsonConvert.DeserializeObject<Reservation>(stream);
            }

            return reservation;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the applied reservation.
        /// </summary>
        /// <returns>The applied reservation.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<ReservationApplied> GetAppliedReservation(string id)
        {
            ReservationApplied reservation = null;

            _logger.Debug("GetAppliedReservation: " + _client.BaseAddress + "api/v1/reservation/applied/" + id);
            var response = await _client.GetAsync("api/v1/reservation/applied/" + id);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                reservation = JsonConvert.DeserializeObject<ReservationApplied>(stream);
            }

            return reservation;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<Device> GetDevice(string id)
        {
            Device device = null;

            _logger.Debug("GetDevice: " + _client.BaseAddress + "api/v1/device/" + id);
            var response = await _client.GetAsync("api/v1/device/" + id);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                device = JsonConvert.DeserializeObject<Device>(stream);
            }

            return device;
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        public async Task<Device> UpdateDevice(Device device)
        {
            var serializedDevice = JsonConvert.SerializeObject(device);
            _logger.Debug(serializedDevice);
            var httpContent = new StringContent(serializedDevice, Encoding.UTF8, "application/json");

            _logger.Debug("UpdateDevice: " + _client.BaseAddress + "api/v1/device/" + device.Id);
            var response = await _client.PutAsync("api/v1/device/" + device.Id, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return await GetDevice(device.Id);
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the configuration from DB.
        /// </summary>
        /// <returns>The configuration.</returns>
        public async Task<IManagerConfiguration> GetManagerConfiguration()
        {
            var response = await _client.GetAsync("api/v1/configuration");

            var stream = await response.Content.ReadAsStringAsync();
            var configuration = JsonConvert.DeserializeObject<ManagerConfiguration>(stream);

            return configuration;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the appium processes.
        /// </summary>
        /// <returns>The appium processes.</returns>
        public async Task<IEnumerable<AppiumProcess>> GetAppiumProcesses()
        {
            IEnumerable<AppiumProcess> appiumProcesses = new List<AppiumProcess>();

            var response = await _client.GetAsync("api/v1/appium");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                appiumProcesses = JsonConvert.DeserializeObject<IEnumerable<AppiumProcess>>(stream);
            }

            return appiumProcesses;
        }

        /// <summary>
        /// Gets the appium process.
        /// </summary>
        /// <returns>The appium process.</returns>
        /// <param name="id">Identifier.</param>
        private async Task<AppiumProcess> GetAppiumProcess(string id)
        {
            AppiumProcess appiumProcess = null;

            _logger.Debug("GetDevice: " + _client.BaseAddress + "api/v1/appium/" + id);
            var response = await _client.GetAsync("api/v1/appium/" + id);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStringAsync();
                appiumProcess = JsonConvert.DeserializeObject<AppiumProcess>(stream);
            }

            return appiumProcess;
        }

        /// <summary>
        /// Adds the appium process.
        /// </summary>
        /// <returns>The appium process.</returns>
        /// <param name="appiumProcess">Appium process.</param>
        public async Task<AppiumProcess> AddAppiumProcess(AppiumProcess appiumProcess)
        {
            _logger.Info("AddAppiumProcess Thread started");
            var serializedAppiumProcess = JsonConvert.SerializeObject(appiumProcess);
            _logger.Debug(serializedAppiumProcess);
            var httpContent = new StringContent(serializedAppiumProcess, Encoding.UTF8, "application/json");

            _logger.Debug("AddAppiumProcess: " + _client.BaseAddress + "api/v1/appium");
            var response = await _client.PostAsync("api/v1/appium", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            return await GetAppiumProcess(appiumProcess.DeviceId);
        }

        /// <summary>
        /// Removes the appium process.
        /// </summary>
        /// <returns>The appium process.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<bool> RemoveAppiumProcess(string id)
        {
            _logger.Debug("RemoveAppiumProcess: " + _client.BaseAddress + "api/v1/appium/" + id);
            var response = await _client.DeleteAsync("api/v1/appium/" + id);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(response.StatusCode.ToString());
                return false;
            }

            return true;
        }
    }
}
