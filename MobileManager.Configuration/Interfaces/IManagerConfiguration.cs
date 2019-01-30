namespace MobileManager.Configuration.Interfaces
{
    /// <summary>
    /// Manager configuration.
    /// </summary>
    public interface IManagerConfiguration
    {
        /// <summary>
        /// Gets or sets the listening port.
        /// </summary>
        /// <value>The listening port.</value>
        int ListeningPort { get; }

        /// <summary>
        /// Gets or sets the listening IP address.
        /// </summary>
        /// <value>The listening ip address.</value>
        string ListeningIpAddress { get; }

        /// <summary>
        /// Gets or sets the local ip address.
        /// </summary>
        string LocalIpAddress { get; }

        /// <summary>
        /// Gets or sets the IOS Device service refresh time.
        /// </summary>
        /// <value>The IOS Device service refresh time.</value>
        int IosDeviceServiceRefreshTime { get; }

        /// <summary>
        /// Gets or sets the reservation service refresh time.
        /// </summary>
        /// <value>The reservation service refresh time.</value>
        int ReservationServiceRefreshTime { get; }

        /// <summary>
        /// Gets or sets the appium port range minimum.
        /// </summary>
        /// <value>The appium port range minimum.</value>
        int AppiumPortRangeMin { get; }

        /// <summary>
        /// Gets or sets the appium port range max.
        /// </summary>
        /// <value>The appium port range max.</value>
        int AppiumPortRangeMax { get; }

        /// <summary>
        /// Gets or sets the appium log file path.
        /// </summary>
        /// <value>The appium log file path.</value>
        string AppiumLogFilePath { get; }

        /// <summary>
        /// Gets or sets the idevice syslog folder path.
        /// </summary>
        /// <value>The idevice syslog folder path.</value>
        string IdeviceSyslogFolderPath { get; }

        /// <summary>
        /// Gets or sets the android device service refresh time.
        /// </summary>
        /// <value>The android device service refresh time.</value>
        int AndroidDeviceServiceRefreshTime { get; }

        /// <summary>
        /// Gets or sets the global reconnect timeout.
        /// </summary>
        /// <value>The global reconnect timeout.</value>
        int GlobalReconnectTimeout { get; }

        /// <summary>
        /// Gets the project version.
        /// </summary>
        /// <value>The project version.</value>
        string ProjectVersion { get; }

        /// <summary>
        /// Gets or sets the xcode path.
        /// </summary>
        /// <value>The xcode path.</value>
        string XcodePath { get; }

        /// <summary>
        /// Gets the Android service enabled value.
        /// </summary>
        bool AndroidServiceEnabled { get; }

        /// <summary>
        /// Gets the IOS service enabled value.
        /// </summary>
        bool IosServiceEnabled { get; }

        /// <summary>
        /// iOS developer certificate token
        /// </summary>
        string IosDeveloperCertificateTeamId { get; }
    }
}
