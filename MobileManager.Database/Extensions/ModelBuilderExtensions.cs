using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Enums;
using Newtonsoft.Json;

namespace MobileManager.Database.Extensions
{
    /// <summary>
    /// Extensions to <see cref="ModelBuilder"/>
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Retrieves model mapping from DB mapping files for given type
        /// </summary>
        /// <typeparam name="T">DB Model</typeparam>
        /// <param name="builder">Model builder</param>
        /// <returns>Returns model builder</returns>
        public static ModelBuilder GetModelMapping<T>(this ModelBuilder builder) where T : class
        {
            var dbConfiguration = AppConfigurationProvider.Get<DbConfiguration>();
            var mappingFileName = $"{typeof(T).Name}.json";

            var mappingObject =
                JsonConvert.DeserializeObject<ClassDbMapping>(
                    File.ReadAllText(dbConfiguration.DbMappingFilesPath + "/" + mappingFileName));

            builder.HasDefaultSchema(dbConfiguration.DefaultDbSchema);
            builder.Entity<T>().ToTable(mappingObject.TableNames[dbConfiguration.DbProvider]);

            foreach (var propertyMapping in mappingObject.PropertyMappings)
            {
                builder.Entity<T>()
                    .Property(propertyMapping.PropertyName)
                    .HasColumnName(propertyMapping.Mappings[dbConfiguration.DbProvider]);
            }

            return builder;
        }

        /// <summary>
        /// Provides mapping between class property and column names
        /// </summary>
        private class PropertyDbMapping
        {
            public string PropertyName { get; set; }
            public Dictionary<DbProviders, string> Mappings { get; set; }
        }

        /// <summary>
        /// Provides mapping for given class
        /// </summary>
        private class ClassDbMapping
        {
            public Dictionary<DbProviders, string> TableNames { get; set; }
            public List<PropertyDbMapping> PropertyMappings { get; set; }
        }
    }
}
