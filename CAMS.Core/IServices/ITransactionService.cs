using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Transaction;
using CAMS.Core.PresentationModels.ViewModels.Transaction;

namespace CAMS.Core.IServices
{
    // ---------- 4. Transaction Processing ----------
    public interface ITransactionService
    {
        Task<TransactionViewModel> CreateTransactionAsync(string userId, CreateTransactionDto dto, CancellationToken ct = default);
        Task<TransactionViewModel?> GetTransactionByIdAsync(string transactionId, CancellationToken ct = default);
        Task<PaginedResponse<IEnumerable<TransactionViewModel>>> GetTransactionsAsync(TransactionQuery query, CancellationToken ct = default);
    }
}
