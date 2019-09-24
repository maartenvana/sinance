using Sinance.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sinance.Storage
{
    /// <summary>
    /// Generic repository
    /// </summary>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : EntityBase
    {
        private readonly SinanceContext context;

        private readonly DbSet<TEntity> dbSet;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public GenericRepository(SinanceContext dbContext)
        {
            context = dbContext;

            dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="id">Id</param>
        public void Delete(int id)
        {
            TEntity entity = FindSingle(item => item.Id == id);

            if (entity != null)
                Delete(entity);
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public void Delete(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        /// <summary>
        /// Delete collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Found entities</returns>
        public IList<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate)
        {
            IList<TEntity> query = dbSet.Where(predicate).ToList();
            return query;
        }

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <param name="includeProperties">Properties to eagerly load</param>
        /// <returns>Found entities</returns>
        public IList<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
                throw new ArgumentNullException(nameof(includeProperties));

            IQueryable<TEntity> query = dbSet.Where(predicate);
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            return query.ToList();
        }

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Found entities</returns>
        public TEntity FindSingle(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity query = dbSet.SingleOrDefault(predicate);
            return query;
        }

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <param name="includeProperties">Properties to eagerly load</param>
        /// <returns>Found entities</returns>
        public TEntity FindSingle(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
                throw new ArgumentNullException(nameof(includeProperties));

            IQueryable<TEntity> query = dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            TEntity entity = query.SingleOrDefault(predicate);

            return entity;
        }

        public IList<TEntity> FindTopAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, int count)
        {
            var query = dbSet.Where(predicate).OrderBy(orderBy).Take(count).ToList();

            return query;
        }

        public IList<TEntity> FindTopDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count)
        {
            var query = dbSet.Where(predicate).OrderByDescending(orderByDescending).Take(count).ToList();

            return query;
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        /// <summary>
        /// Insert collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        public void InsertRange(IEnumerable<TEntity> entities)
        {
            dbSet.AddRange(entities);
        }

        /// <summary>
        /// List all entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Collection of all entities</returns>
        public IList<TEntity> ListAll()
        {
            IList<TEntity> result = dbSet.ToList();
            return result;
        }

        /// <summary>
        /// List all entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Collection of all entities</returns>
        public IList<TEntity> ListAll(params string[] includeProperties)
        {
            if (includeProperties == null)
                throw new ArgumentNullException(nameof(includeProperties));

            IQueryable<TEntity> query = dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            IList<TEntity> result = query.ToList();

            return result;
        }

        /// <summary>
        /// Save changes
        /// </summary>
        public void Save()
        {
            context.SaveChanges();
        }

        public decimal Sum(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, decimal>> sumPredicate)
        {
            return dbSet.Where(filterPredicate).Sum(sumPredicate);
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public void Update(TEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Update collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of entities</param>
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (TEntity entity in entities)
            {
                Update(entity);
            }
        }

        /// <summary>
        /// Protected dispose
        /// </summary>
        /// <param name="disposing">Disposing boolean</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (context != null)
                {
                    context.Dispose();
                }
            }
        }
    }
}