using Microsoft.EntityFrameworkCore;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Database.Extensions;

namespace MobileManager.Database.DatabaseContexts.Base
{
    public class MultiDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbconfig = AppConfigurationProvider.Get<DbConfiguration>();
            optionsBuilder.UseMultiDb(dbconfig.ConnectionStrings[dbconfig.DbProvider]);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}
