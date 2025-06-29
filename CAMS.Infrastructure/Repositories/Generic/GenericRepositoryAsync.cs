using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Generic;
using CAMS.Infrastructure.Data.context;
using CAMS.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace CAMS.Infrastructure.Repositories.Generic
{
    public class GenericRepositoryAsync<TEntity, TPrimaryKey> : IGenericRepositoryAsync<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _entities;

        public GenericRepositoryAsync(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = _context.Set<TEntity>();
        }

        public async Task<PaginedResponse<IEnumerable<TEntity>>> GetAllAsync(
           Expression<Func<TEntity, bool>> condition = null,
           Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes = null,
           Expression<Func<TEntity, object>> orderBy = null,
           bool isAscending = true,
           int pageNumber = 1,
           int pageSize = 10,
           CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _entities;

            if (condition != null)
                query = query.Where(condition);

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => include(current));

            if (orderBy != null)
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            int totalItems = await query.CountAsync();

            pageNumber = Math.Max(pageNumber, 1);
            int skip = (pageNumber - 1) * pageSize;

            var items = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginedResponse<IEnumerable<TEntity>>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }


        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _entities.ToListAsync(cancellationToken);
        }


        public async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(condition, cancellationToken);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _entities.AddAsync(entity, cancellationToken);
        }


        public Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _entities.Update(entity);
            return Task.CompletedTask;
        }


        public Task DeleteAsync(TEntity entity)
        {
            _entities.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<TEntity> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[]? includes = null,
            CancellationToken cancellationToken = default)
        {
            if (condition is null) throw new ArgumentNullException(nameof(condition));

            IQueryable<TEntity> query = _entities;

            if (includes?.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.FirstOrDefaultAsync(condition, cancellationToken);
        }

        public Task UpdateRangeAsync(IEnumerable<TEntity> entity)
        {
            _entities.UpdateRange(entity);
            return Task.CompletedTask;
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes = null, Expression<Func<TEntity, object>> orderBy = null, bool isAscending = true)
        {
            IQueryable<TEntity> query = _entities;

            if (condition != null)
                query = query.Where(condition);

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => include(current));

            if (orderBy != null)
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);



            var item = await query.FirstOrDefaultAsync();

            return item;
        }
    }

}
