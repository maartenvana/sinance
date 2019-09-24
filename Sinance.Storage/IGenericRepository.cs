using Sinance.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

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
        /// <param name="id">Id</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type parameter needed")]
        void Delete(int id);

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
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Found entities</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        IList<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        IList<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties);

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Found entities</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        TEntity FindSingle(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <param name="includeProperties">What properties to include</param>
        /// <returns>Found entities</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        TEntity FindSingle(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties);

        IList<TEntity> FindTopAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByAscending, int count);

        IList<TEntity> FindTopDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count);

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

        IList<TEntity> ListAll();

        IList<TEntity> ListAll(params string[] includeProperties);

        /// <summary>
        /// Save changes
        /// </summary>
        void Save();

        decimal Sum(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, decimal>> sumPredicate);

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