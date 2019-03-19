using System;
using System.Collections.Generic;
using System.Linq;
using MobileManager.Configuration.Interfaces;

namespace MobileManager.Configuration.ConfigurationProvider
{
    /// <summary>
    /// Registers and provides Configurations
    /// </summary>
    public class AppConfigurationProvider
    {
        private AppConfigurationProvider()
        {
        }

        private static AppConfigurationProvider _instance;
        private static List<IConfiguration> _configurations;

        /// <summary>
        /// Registers Configuration for initialization and future use
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <returns>ConfigurationProvider</returns>
        public static AppConfigurationProvider Register<T>(string configPath) where T : IConfiguration
        {
            if (_instance == null)
            {
                _instance = new AppConfigurationProvider();
                _configurations = new List<IConfiguration>();
            }

            if (!_configurations.Exists(x => x.GetType() == typeof(T)))
            {
                _configurations.Add(Activator.CreateInstance<T>().Load(configPath));
            }

            return _instance;
        }

        /// <summary>
        /// Registers Configuration for initialization and future use
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <returns>ConfigurationProvider</returns>
        public AppConfigurationProvider RegisterNext<T>(string configPath) where T : IConfiguration
        {
            if (!_configurations.Exists(x => x.GetType() == typeof(T)))
            {
                _configurations.Add(Activator.CreateInstance<T>().Load(configPath));
            }

            return _instance;
        }

        /// <summary>
        /// Gets instance of registered configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : IConfiguration
        {
            if (_configurations == null)
            {
                throw new InvalidOperationException(
                    "Configurations were not initiliazed. Call ConfigurationProvider.Register<T>() first.");
            }

            var registeredConfiguration = _configurations.FirstOrDefault(x => x.GetType() == typeof(T));

            if (registeredConfiguration == null)
            {
                throw new InvalidOperationException(
                    "Configuration not registered. Use ConfigurationProvider.Register<T>() first.");
            }

            return (T) registeredConfiguration.Clone();
        }
    }
}
