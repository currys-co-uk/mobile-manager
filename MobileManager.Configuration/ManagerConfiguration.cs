using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using MobileManager.Configuration.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Configuration
{
    /// <inheritdoc cref="IManagerConfiguration"/>
    /// <summary>
    /// Manager configuration.
    /// </summary>
    public class ManagerConfiguration : IManagerConfiguration, IConfiguration
    {
        private string _localIpAddress;

        public string LocalIpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(ListeningIpAddress))
                {
                    _localIpAddress = ListeningIpAddress;
                }

                return _localIpAddress ?? (_localIpAddress = GetLocalIp());
            }
            private set => _localIpAddress = value;
        }

        /// <inheritdoc />
        [JsonProperty]
        public string ListeningIpAddress { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int ListeningPort { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int IosDeviceServiceRefreshTime { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int ReservationServiceRefreshTime { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int AndroidDeviceServiceRefreshTime { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int GlobalReconnectTimeout { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int AppiumPortRangeMin { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public int AppiumPortRangeMax { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public string AppiumLogFilePath { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public string IdeviceSyslogFolderPath { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public string IosDeveloperCertificateTeamId { get; private set; }

        /// <inheritdoc />
        public string ProjectVersion
        {
            get
            {
                var version = (AssemblyInformationalVersionAttribute) Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute)).FirstOrDefault();

                return version?.InformationalVersion;
                //get => Application.ProductVersion.ToString();
            }
        }

        /// <inheritdoc />
        [JsonProperty]
        public string XcodePath { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public bool AndroidServiceEnabled { get; private set; }

        /// <inheritdoc />
        [JsonProperty]
        public bool IosServiceEnabled { get; private set; }

        private string GetLocalIp()
        {
            var ipv4Address = string.Empty;

            foreach (var currentIpAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (currentIpAddress.ToString().StartsWith("169"))
                {
                    continue;
                }

                if (currentIpAddress.AddressFamily.ToString() ==
                    AddressFamily.InterNetwork.ToString())
                {
                    ipv4Address = currentIpAddress.ToString();
                    break;
                }
            }

            return ipv4Address;
        }

        public IConfiguration Load(string configPath)
        {
            return (ManagerConfiguration) JsonConvert.DeserializeObject(File.ReadAllText(configPath),
                typeof(ManagerConfiguration));
        }

        public IConfiguration Clone()
        {
            return new ManagerConfiguration
            {
                LocalIpAddress = LocalIpAddress,
                AndroidDeviceServiceRefreshTime = AndroidDeviceServiceRefreshTime,
                IosDeviceServiceRefreshTime = IosDeviceServiceRefreshTime,
                ReservationServiceRefreshTime = ReservationServiceRefreshTime,
                AppiumLogFilePath = AppiumLogFilePath,
                AppiumPortRangeMin = AppiumPortRangeMin,
                AppiumPortRangeMax = AppiumPortRangeMax,
                GlobalReconnectTimeout = GlobalReconnectTimeout,
                IdeviceSyslogFolderPath = IdeviceSyslogFolderPath,
                XcodePath = XcodePath,
                IosDeveloperCertificateTeamId = IosDeveloperCertificateTeamId,
                ListeningPort = ListeningPort,
                ListeningIpAddress = ListeningIpAddress,
                AndroidServiceEnabled = AndroidServiceEnabled,
                IosServiceEnabled = IosServiceEnabled
            };
        }
    }
}
