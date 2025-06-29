using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;

namespace CAMS.Core.IServices
{
    // ---------- 7. Savings Account ----------
    public interface ISavingsService
    {
        Task<bool> OpenSavingsAsync(string clientId, CreateSavingsDto dto, CancellationToken ct = default);
        Task EnableOverdraftAsync(string savingsAccountId, bool enable, CancellationToken ct = default);
        Task<PaginedResponse<IEnumerable<SavingViewModel>>> GetSavingsAccountsAsync(SavingsQuery query, CancellationToken ct = default);
        Task<SavingViewModel> GetSavingAccountAsync(string savingId, CancellationToken ct = default);
    }
}
