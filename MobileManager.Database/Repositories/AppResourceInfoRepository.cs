using System.Collections.Generic;
using System.Linq;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.App;

namespace MobileManager.Database.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// AppResourceInfo repository.
    /// </summary>
    public class AppResourceInfoRepository : IRepository<AppResourceInfo>
    {
        readonly GeneralDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Database.Repositories.AppResourceInfo"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public AppResourceInfoRepository(GeneralDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        public IEnumerable<AppResourceInfo> GetAll()
        {
            IEnumerable<AppResourceInfo> appResources;
            appResources = _context.AppResources.OrderBy(d => d.Id).ToList();

            return appResources;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the specified AppResourceInfo.
        /// </summary>
        /// <param name="appResourceInfo">AppResourceInfo.</param>
        public void Add(AppResourceInfo appResourceInfo)
        {
            _context.Add(appResourceInfo);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        /// <summary>
        /// Add the list of AppResourceInfo.
        /// </summary>
        /// <param name="entities">List of AppResourceInfo.</param>
        public void Add(IEnumerable<AppResourceInfo> entities)
        {
            _context.AddRange(entities);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        /// <summary>
        /// Find the specified id.
        /// </summary>
        /// <returns>The found AppResourceInfo.</returns>
        /// <param name="id">Identifier.</param>
        public AppResourceInfo Find(string id)
        {
            var appResourceInfo = _context.AppResources.Find(id);
            return appResourceInfo;
        }

        /// <inheritdoc />
        /// <summary>
        /// Remove the specified id.
        /// </summary>
        /// <returns>Boolean of success.</returns>
        /// <param name="id">Identifier.</param>
        public bool Remove(string id)
        {
            var appResourceInfo = Find(id);
            if (appResourceInfo == null)
            {
                return false;
            }

            _context.AppResources.Remove(appResourceInfo);
            _context.SaveChanges();

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Update the specified AppResourceInfo.
        /// </summary>
        /// <param name="appResourceInfo">AppResourceInfo.</param>
        public void Update(AppResourceInfo appResourceInfo)
        {
            _context.AppResources.Find(appResourceInfo.Id);
            _context.Update(appResourceInfo);
            _context.SaveChanges();
        }
    }
}
