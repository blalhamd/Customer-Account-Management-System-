using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;

namespace CAMS.Core.IServices
{
    // ---------- 5. Loan Workflow ----------
    public interface ILoanService
    {
        Task<bool> ApplyForLoanAsync(string clientId, CreateLoanDto dto, CancellationToken ct = default);
        Task ApproveLoanAsync(string loanAccountId, CancellationToken ct = default);

        Task<PaginedResponse<IEnumerable<LoanViewModel>>> GetLoansAsync(LoanQuery query, CancellationToken ct = default);
        Task<LoanViewModel?> GetLoanByIdAsync(string loanAccountId, CancellationToken ct = default);
        Task<bool> MakeInstallmentPaymentAsync(string loanId, decimal amount, string sourceAccountId, CancellationToken ct = default);
    }
}
