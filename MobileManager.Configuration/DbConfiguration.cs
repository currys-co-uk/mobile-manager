using System.Collections.Generic;
using System.IO;
using MobileManager.Configuration.Enums;
using MobileManager.Configuration.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Configuration
{
    public class DbConfiguration : IConfiguration
    {
        [JsonProperty] public string DefaultDbSchema { get; private set; }

        [JsonProperty] public DbProviders DbProvider { get; private set; }

        [JsonProperty] public Dictionary<DbProviders, string> ConnectionStrings { get; private set; }

        [JsonProperty] public string DbMappingFilesPath { get; private set; }

        public IConfiguration Load(string configPath)
        {
            return (DbConfiguration) JsonConvert.DeserializeObject(File.ReadAllText(configPath),
                typeof(DbConfiguration));
        }

        public IConfiguration Clone()
        {
            return new DbConfiguration
            {
                DefaultDbSchema = DefaultDbSchema,
                DbProvider = DbProvider,
                ConnectionStrings = new Dictionary<DbProviders, string>(ConnectionStrings),
                DbMappingFilesPath = DbMappingFilesPath
            };
        }
    }
}
