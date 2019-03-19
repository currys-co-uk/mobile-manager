using System;
using Microsoft.EntityFrameworkCore;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Enums;

namespace MobileManager.Database.Extensions
{
    /// <summary>
    /// Extensions for DbContextOptionsBuilder
    /// </summary>
    public static class DbContextOptionsBuildExtensions
    {
        /// <summary>
        /// Uses provider specific extension based on configured provider in dbconfig.json
        /// </summary>
        /// <param name="builder">Context options builder</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns Context options builder</returns>
        public static DbContextOptionsBuilder UseMultiDb(this DbContextOptionsBuilder builder, string connectionString)
        {
            var dbconfig = AppConfigurationProvider.Get<DbConfiguration>();
            switch (dbconfig.DbProvider)
            {
                case DbProviders.MsSql:
                    return builder.UseSqlServer(connectionString);
                case DbProviders.PostgreSql:
                    return builder.UseNpgsql(connectionString
                    );
                case DbProviders.Sqlite:
                    return builder.UseSqlite(connectionString);
                default:
                    throw new InvalidOperationException("Unknown DB provider setup in database configuration.");
            }
        }
    }
}
