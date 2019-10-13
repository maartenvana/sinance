using Sinance.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sinance.Domain.Entities;

namespace Sinance.Storage
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : EntityBase
    {
        private readonly SinanceContext _context;

        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(SinanceContext dbContext)
        {
            _context = dbContext;

            _dbSet = _context.Set<TEntity>();
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<List<TEntity>> FindAll(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            var query = _dbSet.Where(predicate);

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<List<TEntity>> FindAllTracked(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            var query = _dbSet.Where(predicate);

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.AsTracking().ToListAsync();
        }

        public async Task<TEntity> FindSingle(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            var query = _dbSet.AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<TEntity> FindSingleTracked(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            var query = _dbSet.AsTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<List<TEntity>> FindTopAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByAscending, int count, params string[] includeProperties)
        {
            var query = _dbSet
                 .Where(predicate)
                 .OrderBy(orderByAscending)
                 .Take(count)
                 .AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopAscendingTracked(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, int count, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(count)
                .AsTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(predicate)
                .OrderByDescending(orderByDescending)
                .Take(count)
                .AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopDescendingTracked(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(predicate)
                .OrderByDescending(orderByDescending)
                .Take(count)
                .AsTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.ToListAsync();
        }

        public void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void InsertRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public async Task<List<TEntity>> ListAll(params string[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<List<TEntity>> ListAllTracked(params string[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.AsTracking().ToListAsync();
        }

        public async Task<decimal> Sum(Expression<Func<TEntity, bool>> filterQuery, Expression<Func<TEntity, decimal>> sumQuery)
        {
            return await _dbSet.Where(filterQuery).SumAsync(sumQuery);
        }

        public void Update(TEntity entity)
        {
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                }
            }
        }
    }
}