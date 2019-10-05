using Sinance.Domain;
using Microsoft.EntityFrameworkCore;
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

        public void Delete(int id)
        {
            var entity = FindSingleTracked(item => item.Id == id);

            if (entity != null)
            {
                Delete(entity);
            }
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _dbSet.Where(predicate).AsNoTracking().ToList();
            return query;
        }

        public List<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            var query = _dbSet.Where(predicate);
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            return query.AsNoTracking().ToList();
        }

        public List<TEntity> FindAllTracked(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _dbSet.Where(predicate).AsTracking().ToList();
            return query;
        }

        public List<TEntity> FindAllTracked(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            var query = _dbSet.Where(predicate);
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            return query.AsTracking().ToList();
        }

        public TEntity FindSingle(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _dbSet.AsNoTracking().SingleOrDefault(predicate);
            return query;
        }

        public TEntity FindSingle(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            IQueryable<TEntity> query = _dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            var entity = query.AsNoTracking().SingleOrDefault(predicate);

            return entity;
        }

        public TEntity FindSingleTracked(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _dbSet.AsTracking().SingleOrDefault(predicate);
            return query;
        }

        public TEntity FindSingleTracked(Expression<Func<TEntity, bool>> predicate, params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            IQueryable<TEntity> query = _dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            var entity = query.AsTracking().SingleOrDefault(predicate);

            return entity;
        }

        public List<TEntity> FindTopAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByAscending, int count)
        {
            var query = _dbSet.Where(predicate).OrderBy(orderByAscending).Take(count).AsNoTracking().ToList();

            return query;
        }

        public List<TEntity> FindTopAscendingTracked(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, int count)
        {
            var query = _dbSet.Where(predicate).OrderBy(orderBy).Take(count).AsTracking().ToList();

            return query;
        }

        public List<TEntity> FindTopDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count)
        {
            var query = _dbSet.Where(predicate).OrderByDescending(orderByDescending).Take(count).AsNoTracking().ToList();

            return query;
        }

        public List<TEntity> FindTopDescendingTracked(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderByDescending, int count)
        {
            var query = _dbSet.Where(predicate).OrderByDescending(orderByDescending).Take(count).AsTracking().ToList();

            return query;
        }

        public void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void InsertRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public List<TEntity> ListAll(params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            IQueryable<TEntity> query = _dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            var result = query.AsNoTracking().ToList();

            return result;
        }

        public List<TEntity> ListAll()
        {
            var result = _dbSet.AsNoTracking().ToList();
            return result;
        }

        public List<TEntity> ListAllTracked(params string[] includeProperties)
        {
            if (includeProperties == null)
            {
                throw new ArgumentNullException(nameof(includeProperties));
            }

            IQueryable<TEntity> query = _dbSet;
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            var result = query.AsTracking().ToList();

            return result;
        }

        public List<TEntity> ListAllTracked()
        {
            var result = _dbSet.AsTracking().ToList();
            return result;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public decimal Sum(Expression<Func<TEntity, bool>> filterQuery, Expression<Func<TEntity, decimal>> sumQuery)
        {
            return _dbSet.Where(filterQuery).Sum(sumQuery);
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