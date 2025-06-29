using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;

namespace CAMS.Core.IServices
{
    // ---------- 8. Joint Account ----------
    public interface IJointAccountService
    {
        Task<bool> CreateJointAccountAsync(string clientId, CreateJointAccountDto dto, CancellationToken ct = default);
        Task AddSecondaryHolderAsync(string jointAccountId, CreateSecondryHolderDto dto, CancellationToken ct = default);
        Task RemoveSecondaryHolderAsync(string jointAccountId, string secondaryClientId, CancellationToken ct = default);
        Task<PaginedResponse<IEnumerable<JointAccountViewModel>>> GetJointAccountsAsync(JointAccountQuery query, CancellationToken ct = default);
    }
}
