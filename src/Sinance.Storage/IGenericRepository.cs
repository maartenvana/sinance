using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sinance.Storage
{
    /// <summary>
    /// Interface for the GenenericRepository
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : EntityBase
    {
        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Delete collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        void DeleteRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Find entities by predicate without tracking
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="findQuery">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindAll(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties);

        /// <summary>
        /// Find entities by predicate with tracking
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="findQuery">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindAllTracked(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties);

        /// <summary>
        /// Find single entity without tracking
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="findQuery">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        Task<TEntity> FindSingle(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties);

        /// <summary>
        /// Find single entity with tracking
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="findQuery">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        Task<TEntity> FindSingleTracked(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties);

        /// <summary>
        /// Finds the top entities sorted ascending without tracking
        /// </summary>
        /// <param name="findQuery">Predicate</param>
        /// <param name="orderByAscending">The order to ascend by</param>
        /// <param name="count">How many to find</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindTopAscending(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByAscending, int count, int skip, params string[] includeProperties);

        /// <summary>
        /// Finds the top entities sorted ascending with tracking
        /// </summary>
        /// <param name="findQuery">Predicate</param>
        /// <param name="orderByAscending">The order to ascend by</param>
        /// <param name="count">How many to find</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindTopAscendingTracked(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByAscending, int count, int skip, params string[] includeProperties);

        /// <summary>
        /// Finds the top entities sorted descending without tracking
        /// </summary>
        /// <param name="findQuery">Predicate</param>
        /// <param name="orderByAscending">The order to descend by</param>
        /// <param name="count">How many to find</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindTopDescending(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByDescending, int count, int skip, params string[] includeProperties);

        /// <summary>
        /// Finds the top entities sorted descending with tracking
        /// </summary>
        /// <param name="findQuery">Predicate</param>
        /// <param name="orderByAscending">The order to descend by</param>
        /// <param name="count">How many to find</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> FindTopDescendingTracked(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByDescending, int count, int skip, params string[] includeProperties);

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        void Insert(TEntity entity);

        /// <summary>
        /// Insert collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        void InsertRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Lists all entities without tracking
        /// </summary>
        /// <param name="includeProperties">Properties to include with query</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> ListAll(params string[] includeProperties);

        /// <summary>
        /// Lists all entities with tracking
        /// </summary>
        /// <param name="includeProperties">Properties to include with query</param>
        /// <returns>Found entities</returns>
        Task<List<TEntity>> ListAllTracked(params string[] includeProperties);

        /// <summary>
        /// Returns the sum of a given field from an entity
        /// </summary>
        /// <param name="findQuery">Query to filter with</param>
        /// <param name="sumQuery">Query to sum by</param>
        /// <returns>Sum of the given field</returns>
        Task<decimal> Sum(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, decimal>> sumQuery);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        void Update(TEntity entity);

        /// <summary>
        /// Update collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        void UpdateRange(IEnumerable<TEntity> entities);
    }
}