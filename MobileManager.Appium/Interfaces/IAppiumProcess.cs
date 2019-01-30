using System.ComponentModel.DataAnnotations;

namespace MobileManager.Appium.Interfaces
{
    /// <summary>
    /// Appium process.
    /// </summary>
    public interface IAppiumProcess
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        [Key]
        string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the appium port.
        /// </summary>
        /// <value>The appium port.</value>
        string AppiumPort { get; set; }

        /// <summary>
        /// Gets or sets the appium bootstrap port.
        /// </summary>
        /// <value>The appium bootstrap port.</value>
        string AppiumBootstrapPort { get; set; }

        /// <summary>
        /// Gets or sets the appium pid.
        /// </summary>
        /// <value>The appium pid.</value>
        int AppiumPid { get; set; }

        /// <summary>
        /// Gets or sets the webkit debug proxy port.
        /// </summary>
        /// <value>The webkit debug proxy port.</value>
        string WebkitDebugProxyPort { get; set; }

        /// <summary>
        /// Gets or sets the wda local port.
        /// </summary>
        /// <value>The wda local port.</value>
        string WdaLocalPort { get; set; }
    }
}
