using System;
using MobileManager.Models.Devices;
using MobileManager.Models.Devices.Interfaces;
using MobileManager.SeleniumConfigs.DataSets.Interfaces;
using DotLiquid;

namespace MobileManager.SeleniumConfigs.DataSets
{
    /// <inheritdoc />
    /// <summary>
    /// AndroidSeleniumConfig.
    /// This class is dataset for DotLiquid text templating system.
    /// Inherits from DotLiquid/Drop class for data transform.
    /// </summary>
    public class AndroidSeleniumConfig : Drop, ISeleniumConfig
    {
        /// <summary>
        /// Initializes a new instance of the AndroidSeleniumConfig class.
        /// </summary>
        /// <param name="device">IDevice Device</param>
        public AndroidSeleniumConfig(IDevice device) { 
            this.Id = device.Id;
            this.Name = device.Name;
            this.AppiumEndpoint = device.AppiumEndpoint;
            this.Host = !string.IsNullOrEmpty(device.AppiumEndpoint) ? new Uri(device.AppiumEndpoint).Host : string.Empty;
            this.Port = !string.IsNullOrEmpty(device.AppiumEndpoint) ? new Uri(device.AppiumEndpoint).Port.ToString() : string.Empty;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Id.
        /// </summary>
        /// <value>The Id.</value>
        public string Id { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Name.
        /// </summary>
        /// <value>The Name.</value>
        public string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the AppiumEndpoint.
        /// </summary>
        /// <value>The AppiumEndpoint.</value>
        public string AppiumEndpoint { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Host.
        /// </summary>
        /// <value>The Host.</value>
        public string Host { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Port.
        /// </summary>
        /// <value>The Port.</value>
        public string Port { get; }
    }

}