using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;

namespace CAMS.Core.IServices
{
    // ---------- 6. Fixed Deposit ----------
    public interface IFixedDepositService
    {
        Task<bool> OpenFixedDepositAsync(string clientId, CreateFixedDepositDto dto, CancellationToken ct = default);
        Task MatureFixedDepositAsync(CancellationToken cancellation = default); // Hangfire
        Task<PaginedResponse<IEnumerable<FixedDepositViewModel>>> GetFixedDepositsAsync(FixedDepositQuery query, CancellationToken ct = default);
    }
}
