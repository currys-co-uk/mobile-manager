using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.Devices;

namespace MobileManager.Database.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Device repository.
    /// </summary>
    public class DeviceRepository : IRepository<Device>
    {
        readonly GeneralDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Database.Repositories.DeviceRepository"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public DeviceRepository(GeneralDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<Device> GetAll()
        {
            IEnumerable<Device> devices;
            using (var scope = new TransactionScope())
            {
                devices = _context.Devices.Include(d => d.Properties).OrderBy(d => d.Id).ToList();
                scope.Complete();
            }

            return devices;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the specified device.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="device">Device.</param>
        public void Add(Device device)
        {
            using (var scope = new TransactionScope())
            {
                _context.Add(device);
                _context.SaveChanges();

                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void Add(IEnumerable<Device> entities)
        {
            using (var scope = new TransactionScope())
            {
                _context.AddRange(entities);
                _context.SaveChanges();

                scope.Complete();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Find the specified id.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="id">Identifier.</param>
        public Device Find(string id)
        {
            Device device;

            using (var scope = new TransactionScope())
            {
                device = _context.Devices.Include(d => d.Properties).FirstOrDefault(r => r.Id == id);

                scope.Complete();
            }

            return device;
        }

        /// <inheritdoc />
        /// <summary>
        /// Remove the specified id.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="id">Identifier.</param>
        public bool Remove(string id)
        {
            var device = Find(id);
            if (device == null)
            {
                return false;
            }

            using (var scope = new TransactionScope())
            {
                _context.Devices.Remove(device);
                _context.SaveChanges();

                scope.Complete();
            }

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Update the specified device.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="device">Device.</param>
        public void Update(Device device)
        {
            using (var scope = new TransactionScope())
            {
                _context.Devices.Include(d => d.Properties).FirstOrDefault(d => d.Id == device.Id);
                _context.Update(device);
                _context.SaveChanges();

                scope.Complete();
            }
        }
    }
}
