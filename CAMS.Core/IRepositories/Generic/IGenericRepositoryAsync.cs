using CAMS.Core.Constants;
using CAMS.Shared.Interfaces;
using System.Linq.Expressions;

namespace CAMS.Core.IRepositories.Generic
{
    public interface IGenericRepositoryAsync<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PaginedResponse<IEnumerable<TEntity>>> GetAllAsync(
            Expression<Func<TEntity, bool>> condition = null!,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes = null!,
            Expression<Func<TEntity, object>> orderBy = null!,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken cancellationToken = default);

        Task<TEntity> GetByIdAsync(TPrimaryKey id);

        Task<TEntity> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes = null!, CancellationToken cancellationToken = default);

        Task<TEntity> FirstOrDefaultAsync(
           Expression<Func<TEntity, bool>> condition = null!,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes = null!,
            Expression<Func<TEntity, object>> orderBy = null!,
            bool isAscending = true);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity);
        Task UpdateRangeAsync(IEnumerable<TEntity> entity);

        Task DeleteAsync(TEntity entity);
    }
}
