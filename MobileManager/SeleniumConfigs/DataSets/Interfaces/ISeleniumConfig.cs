namespace MobileManager.SeleniumConfigs.DataSets.Interfaces
{
    /// <summary>
    /// SeleniumConfig.
    /// </summary>
    public interface ISeleniumConfig
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the appium endpoint.
        /// </summary>
        /// <value>The appium endpoint.</value>
        string AppiumEndpoint { get; set; }

        /// <summary>
        /// Gets or sets Host.
        /// </summary>
        /// <value>The Host.</value>
        string Host { get; set; }

        /// <summary>
        /// Gets or sets Port.
        /// </summary>
        /// <value>The Port.</value>
        string Port { get; set; }
    }
}
