using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;
using CAMS.Domains.Enums;

namespace CAMS.Core.IServices
{
    // ---------- 3. Account Management ----------
    public interface IAccountService
    {
        Task<PaginedResponse<IEnumerable<AccountViewModel>>> GetAccountsForClientAsync(string clientId, AccountQuery query, CancellationToken ct = default);
        Task<AccountViewViewModel?> GetAccountByIdAsync(string accountId, CancellationToken ct = default);

        Task ChangeAccountStatusAsync(string accountId, AccountStatus newStatus, CancellationToken ct = default);
        Task FlagAccountSignedAsync(string accountId, CancellationToken ct = default);
        Task CloseAccountAsync(string accountId, CancellationToken ct = default);

        // Current / Saving specific limits
        Task ResetMonthlyLimitsAsync(CancellationToken ct = default);
    }
}
