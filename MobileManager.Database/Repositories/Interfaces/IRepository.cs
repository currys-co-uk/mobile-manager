using System.Collections.Generic;

namespace MobileManager.Database.Repositories.Interfaces
{
    /// <summary>
    /// Entity repository interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Add the specified entity.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="entity">Entity.</param>
        void Add(T entity);

        /// <summary>
        /// Add range of entities.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="entities">Entities.</param>
        void Add(IEnumerable<T> entities);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Find the specified id.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="id">Identifier.</param>
        T Find(string id);

        /// <summary>
        /// Remove the specified id.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="id">Identifier.</param>
        bool Remove(string id);

        /// <summary>
        /// Update the specified entity.
        /// </summary>
        /// <returns>The update.</returns>
        /// <param name="entity">entity.</param>
        void Update(T entity);
    }
}
