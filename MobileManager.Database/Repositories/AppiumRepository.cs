using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using MobileManager.Appium;
using MobileManager.Database.Repositories.Interfaces;

namespace MobileManager.Database.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Appium repository.
    /// </summary>
    public class AppiumRepository : IRepository<AppiumProcess>
    {
        readonly GeneralDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Database.Repositories.AppiumRepository"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public AppiumRepository(GeneralDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<AppiumProcess> GetAll()
        {
            IEnumerable<AppiumProcess> appiumProcesses;
            using (var scope = new TransactionScope())
            {
                appiumProcesses = _context.AppiumProcess.ToList();

                scope.Complete();
            }

            return appiumProcesses;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the specified appiumProcess.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="appiumProcess">Appium process.</param>
        public void Add(AppiumProcess appiumProcess)
        {
            using (var scope = new TransactionScope())
            {
                _context.AppiumProcess.Add(appiumProcess);
                _context.SaveChanges();

                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void Add(IEnumerable<AppiumProcess> entities)
        {
            using (var scope = new TransactionScope())
            {
                _context.AppiumProcess.AddRange(entities);
                _context.SaveChanges();

                scope.Complete();
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Find the specified deviceId.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="deviceId">Device identifier.</param>
        public AppiumProcess Find(string deviceId)
        {
            AppiumProcess appiumProcess;

            using (var scope = new TransactionScope())
            {
                appiumProcess = _context.AppiumProcess.FirstOrDefault(r => r.DeviceId == deviceId);

                scope.Complete();
            }

            return appiumProcess;
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates stored Appium Process instance
        /// </summary>
        /// <param name="appiumProcess">Appium process</param>
        public void Update(AppiumProcess appiumProcess)
        {
            using (var scope = new TransactionScope())
            {
                if (Remove(appiumProcess.DeviceId))
                {
                    Add(appiumProcess);
                }

                scope.Complete();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Remove the specified deviceId.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="deviceId">Device identifier.</param>
        public bool Remove(string deviceId)
        {
            var appiumProcess = Find(deviceId);
            if (appiumProcess == null)
            {
                return false;
            }

            using (var scope = new TransactionScope())
            {
                _context.AppiumProcess.Remove(appiumProcess);
                _context.SaveChanges();

                scope.Complete();
            }

            return true;
        }
    }
}
