using System.IO;
using Microsoft.Extensions.Logging;
using MobileManager.Configuration.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// Application configuration
    /// </summary>
    public class AppConfiguration : IConfiguration
    {
        /// <summary>
        /// Configure scopes in logs
        /// </summary>
        [JsonProperty]
        public bool IncludeScopes { get; private set; }

        /// <summary>
        /// Default logging level
        /// </summary>
        [JsonProperty]
        public LogLevel DefaultLogLevel { get; private set; }

        public IConfiguration Load(string configPath)
        {
            return (AppConfiguration) JsonConvert.DeserializeObject(File.ReadAllText(configPath),
                typeof(AppConfiguration));
        }

        public IConfiguration Clone()
        {
            return new AppConfiguration
            {
                IncludeScopes = IncludeScopes,
                DefaultLogLevel = DefaultLogLevel
            };
        }
    }
}
