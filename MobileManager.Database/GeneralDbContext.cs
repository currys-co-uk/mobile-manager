using Microsoft.EntityFrameworkCore;
using MobileManager.Appium;
using MobileManager.Database.DatabaseContexts.Base;
using MobileManager.Database.Extensions;
using MobileManager.Models.App;
using MobileManager.Models.Devices;
using MobileManager.Models.Logger;
using MobileManager.Models.Reservations;

namespace MobileManager.Database
{
    public class GeneralDbContext : MultiDbContext
    {
        /// <inheritdoc />
        /// <summary>
        /// On model creating.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.GetModelMapping<Reservation>()
                .GetModelMapping<RequestedDevices>()
                .GetModelMapping<DeviceProperties>()
                .GetModelMapping<ReservationApplied>()
                .GetModelMapping<ReservedDevice>()
                .GetModelMapping<AppiumProcess>()
                .GetModelMapping<Device>()
                .GetModelMapping<AppResourceInfo>();

            // Cascade Delete
            modelBuilder.Entity<Reservation>()
                .HasMany(b => b.RequestedDevices)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequestedDevices>()
                .HasMany(b => b.Properties)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservationApplied>()
                .HasMany(b => b.ReservedDevices)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>The reservations.</value>
        public DbSet<Reservation> Reservations { get; set; }

        /// <summary>
        /// Gets or sets the requested devices.
        /// </summary>
        /// <value>The requested devices.</value>
        public DbSet<RequestedDevices> RequestedDevices { get; set; }

        /// <summary>
        /// Gets or sets the reservations applied.
        /// </summary>
        /// <value>The reservations applied.</value>
        public DbSet<ReservationApplied> ReservationsApplied { get; set; }

        /// <summary>
        /// Gets or sets the reserved devices.
        /// </summary>
        /// <value>The reserved devices.</value>
        public DbSet<ReservedDevice> ReservedDevices { get; set; }

        /// <summary>
        /// Gets or sets the appium process.
        /// </summary>
        /// <value>The appium process.</value>
        public DbSet<AppiumProcess> AppiumProcess { get; set; }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// Gets or sets the device properties.
        /// </summary>
        /// <value>The requested devices.</value>
        public DbSet<DeviceProperties> Properties { get; set; }

        /// <summary>
        /// Gets or sets the Logger.
        /// </summary>
        public DbSet<LogMessage> Logger { get; set; }

        /// <summary>
        /// Gets or sets the AppResources.
        /// </summary>
        public DbSet<AppResourceInfo> AppResources { get; set; }
    }
}
