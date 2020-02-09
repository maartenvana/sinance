using Microsoft.EntityFrameworkCore;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public async Task<List<TEntity>> FindAll(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties)
        {
            var query = _dbSet.Where(findQuery);

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<List<TEntity>> FindAllTracked(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties)
        {
            var query = _dbSet.Where(findQuery);

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.AsTracking().ToListAsync();
        }

        public async Task<TEntity> FindSingle(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties)
        {
            var query = _dbSet.AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.SingleOrDefaultAsync(findQuery);
        }

        public async Task<TEntity> FindSingleTracked(Expression<Func<TEntity, bool>> findQuery, params string[] includeProperties)
        {
            var query = _dbSet.AsTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.SingleOrDefaultAsync(findQuery);
        }

        public async Task<List<TEntity>> FindTopAscending(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByAscending, int count, int skip, params string[] includeProperties)
        {
            var query = _dbSet
                 .Where(findQuery)
                 .OrderBy(orderByAscending)
                 .Skip(skip)
                 .Take(count)
                 .AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopAscendingTracked(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByAscending, int count, int skip, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(findQuery)
                .OrderBy(orderByAscending)
                .Skip(skip)
                .Take(count)
                .AsTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }

            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopDescending(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByDescending, int count, int skip, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(findQuery)
                .OrderByDescending(orderByDescending)
                .Skip(skip)
                .Take(count)
                .AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            }
            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> FindTopDescendingTracked(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, object>> orderByDescending, int count, int skip, params string[] includeProperties)
        {
            var query = _dbSet
                .Where(findQuery)
                .OrderByDescending(orderByDescending)
                .Take(count)
                .Skip(skip)
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

        public async Task<decimal> Sum(Expression<Func<TEntity, bool>> findQuery, Expression<Func<TEntity, decimal>> sumQuery)
        {
            return await _dbSet.Where(findQuery).SumAsync(sumQuery);
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
    }
}