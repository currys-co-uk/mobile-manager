using System;
using Microsoft.Extensions.DependencyInjection;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Enums;

namespace MobileManager.Database.Extensions
{
    /// <summary>
    /// Extensions for IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Uses provider specific extension based on DB provider set in DB Config
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Returns service collection</returns>
        public static IServiceCollection AddEntityFrameworkMultiDb(this IServiceCollection services)
        {
            var dbconfig = AppConfigurationProvider.Get<DbConfiguration>();
            switch (dbconfig.DbProvider)
            {
                case DbProviders.MsSql:
                    return services.AddEntityFrameworkSqlServer();
                case DbProviders.PostgreSql:
                    return services.AddEntityFrameworkNpgsql();
                case DbProviders.Sqlite:
                    return services.AddEntityFrameworkSqlite();
                default:
                    throw new InvalidOperationException("Unknown DB provider setup in database configuration.");
            }
        }
    }
}
