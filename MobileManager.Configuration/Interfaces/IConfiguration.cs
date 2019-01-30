namespace MobileManager.Configuration.Interfaces
{
    /// <summary>
    /// Configuration interface
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Loads Configuration
        /// </summary>
        /// <param name="configPath">Path to configuration file.</param>
        /// <returns>Returns configuration instance.</returns>
        IConfiguration Load(string configPath);

        /// <summary>
        /// Clones current configuration.
        /// </summary>
        /// <returns>Returns clone of current configuration.</returns>
        IConfiguration Clone();
    }
}
