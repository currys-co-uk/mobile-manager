using System.ComponentModel.DataAnnotations;
using MobileManager.Appium.Interfaces;

namespace MobileManager.Appium
{
    /// <inheritdoc />
    /// <summary>
    /// Appium process.
    /// </summary>
    public class AppiumProcess : IAppiumProcess
    {
        /// <inheritdoc />
        [Key]
        public string DeviceId { get; set; }

        /// <inheritdoc />
        public string AppiumPort { get; set; }

        /// <inheritdoc />
        public string AppiumBootstrapPort { get; set; }

        /// <inheritdoc />
        public int AppiumPid { get; set; }

        /// <inheritdoc />
        public string WebkitDebugProxyPort { get; set; }

        /// <inheritdoc />
        public string WdaLocalPort { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Appium.Appium.AppiumProcess"/> class.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="appiumPort">Appium port.</param>
        /// <param name="appiumBootstrapPort">Appium bootstrap port.</param>
        /// <param name="appiumPid">Appium pid.</param>
        /// <param name="webkitDebugProxyPort">Webkit debug proxy port.</param>
        /// <param name="wdaLocalPort">Wda local port.</param>
        public AppiumProcess(string deviceId, string appiumPort, string appiumBootstrapPort, int appiumPid,
            string webkitDebugProxyPort, string wdaLocalPort)
        {
            DeviceId = deviceId;
            AppiumPort = appiumPort;
            AppiumBootstrapPort = appiumBootstrapPort;
            AppiumPid = appiumPid;
            WebkitDebugProxyPort = webkitDebugProxyPort;
            WdaLocalPort = wdaLocalPort;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Appium.Appium.AppiumProcess"/> class.
        /// </summary>
        public AppiumProcess()
        {
        }
    }
}
