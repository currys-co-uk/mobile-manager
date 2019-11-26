namespace MobileManager.SeleniumConfigs.DataSets.Interfaces
{
    /// <summary>
    /// SeleniumConfig.
    /// </summary>
    public interface ISeleniumConfig
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        
        /// <summary>
        /// Gets the appium endpoint.
        /// </summary>
        /// <value>The appium endpoint.</value>
        string AppiumEndpoint { get; }

        /// <summary>
        /// Gets Host.
        /// </summary>
        /// <value>The Host.</value>
        string Host { get; }

        /// <summary>
        /// Gets Port.
        /// </summary>
        /// <value>The Port.</value>
        string Port { get; }
    }
}
