using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Interfaces;
using DotLiquid;

namespace MobileManager.SeleniumConfigs.DataSets
{
    /// <inheritdoc />
    /// <summary>
    /// AndroidSeleniumConfig.
    /// </summary>
    public class AndroidSeleniumConfig : Drop
    {
        /// <summary>
        /// Initializes a new instance of the AndroidSeleniumConfig class.
        /// </summary>
        /// <param name="device">IDevice Device</param>
        public AndroidSeleniumConfig(IDevice device) { 
            this.Id = device.Id;
            this.Name = device.Name;
            this.AppiumEndpoint = device.AppiumEndpoint;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>The Id.</value>
        public string Id { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>The Name.</value>
        public string Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the AppiumEndpoint.
        /// </summary>
        /// <value>The AppiumEndpoint.</value>
        public string AppiumEndpoint { get; set; }
    }

}