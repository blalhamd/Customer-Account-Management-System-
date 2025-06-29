using CAMS.Domains.Enums;

namespace CAMS.Core.IServices
{
    public interface ILoanPricingService
    {
        /// Returns an annual simple-interest rate
        /// e.g. 0.07  ⇒  7 % per annum
        Task<decimal> GetAnnualRateAsync(CurrencyType currency, decimal amount, CancellationToken ct = default);
        Task<decimal> GetFixedDepositInterestRateAsync(CurrencyType currencyType, int termInMonths, CancellationToken ct);
        Task<CurrentAccountConfig> GetCurrentAccountConfigAsync(CurrencyType currency, bool isBusinessAccount, CancellationToken ct = default);
    }
    /// A simple record that holds every knob the service needs
    public sealed record CurrentAccountConfig(
        int MonthlyTxnLimit,
        decimal MinimumBalance,
        decimal MaximumWithdrawal,
        decimal MonthlyFee);
}
