using Microsoft.EntityFrameworkCore.Storage;

namespace CAMS.Core.IUnit
{
    public interface IUnitOfWorkAsync : IAsyncDisposable
    {
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        IExecutionStrategy CreateExecutionStrategy();
    }
}
