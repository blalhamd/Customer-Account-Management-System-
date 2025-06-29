using CAMS.Core.IServices;
using CAMS.Domains.Enums;

namespace CAMS.Business.Services
{
    public sealed class LoanPricingService : ILoanPricingService
    {
        /*────────────────── 1. LOAN TIERS ──────────────────*/

        private static readonly IReadOnlyDictionary<CurrencyType, Tier[]> LoanTiers =
            new Dictionary<CurrencyType, Tier[]>
            {
                [CurrencyType.USD] = new[]
                {
                new Tier(0m,         10_000m,        0.070m),
                new Tier(10_000.01m, 50_000m,        0.085m),
                new Tier(50_000.01m, decimal.MaxValue, 0.090m)
                },
                [CurrencyType.EUR] = new[]
                {
                new Tier(0m,         10_000m,        0.060m),
                new Tier(10_000.01m, 50_000m,        0.075m),
                new Tier(50_000.01m, decimal.MaxValue, 0.085m)
                }
                // add more currencies here…
            };

        public Task<decimal> GetAnnualRateAsync(
            CurrencyType currency,
            decimal amount,
            CancellationToken ct = default)
        {
            if (!LoanTiers.TryGetValue(currency, out var tiers))
                throw new KeyNotFoundException($"No loan pricing configured for {currency}.");

            var tier = tiers.FirstOrDefault(t => t.Min <= amount && amount <= t.Max);
            if (tier == default)
                throw new InvalidOperationException("Loan amount does not match any pricing tier.");

            return Task.FromResult(tier.Rate);
        }

        /*────────────────── 2. CURRENT-ACCOUNT CONFIG ──────────────────*/

        private static readonly IReadOnlyList<CurrentBand> CurrentBands =
     new List<CurrentBand>
     {
        // Personal USD
        new CurrentBand(
            Currency:           CurrencyType.USD,
            IsBusiness:         false,          // ← fixed
            MonthlyTxnLimit:    8,
            MinimumBalance:     100m,
            MaximumWithdrawal:  25_000m,
            MonthlyFee:         5m),

        // Business USD
        new CurrentBand(
            Currency:           CurrencyType.USD,
            IsBusiness:         true,           // ← fixed
            MonthlyTxnLimit:    30,
            MinimumBalance:     1_000m,
            MaximumWithdrawal:  100_000m,
            MonthlyFee:         25m),

        // Personal EUR
        new CurrentBand(
            Currency:           CurrencyType.EUR,
            IsBusiness:         false,          // ← fixed
            MonthlyTxnLimit:    8,
            MinimumBalance:     90m,
            MaximumWithdrawal:  20_000m,
            MonthlyFee:         4.5m)
     };


        public Task<CurrentAccountConfig> GetCurrentAccountConfigAsync(
            CurrencyType currency,
            bool isBusinessAccount,
            CancellationToken ct = default)
        {
            var row = CurrentBands.FirstOrDefault(b => b.Currency == currency &&
                                                       b.IsBusiness == isBusinessAccount);

            if (row == default)
                throw new InvalidOperationException(
                    $"No current-account config for {currency} / {(isBusinessAccount ? "business" : "personal")}.");

            var cfg = new CurrentAccountConfig(
                MonthlyTxnLimit: row.MonthlyTxnLimit,
                MinimumBalance: row.MinimumBalance,
                MaximumWithdrawal: row.MaximumWithdrawal,
                MonthlyFee: row.MonthlyFee);

            return Task.FromResult(cfg);
        }

        /*────────────────── 3. RECORD TYPES ──────────────────*/

        private readonly record struct Tier(
            decimal Min,
            decimal Max,
            decimal Rate);

        private readonly record struct CurrentBand(
            CurrencyType Currency,
            bool IsBusiness,
            int MonthlyTxnLimit,
            decimal MinimumBalance,
            decimal MaximumWithdrawal,
            decimal MonthlyFee);

        public async Task<decimal> GetFixedDepositInterestRateAsync(
            CurrencyType currencyType, int termInMonths, CancellationToken ct)
        {
            // Example: Fetch the rate from a configuration, a service, or from the database.
            // This logic should be updated to match your application's pricing rules.

            decimal interestRate;

            if (currencyType == CurrencyType.USD)
            {
                if (termInMonths <= 6)
                {
                    interestRate = 0.03m; // 3% interest for less than 6 months
                }
                else if (termInMonths <= 12)
                {
                    interestRate = 0.05m; // 5% interest for terms between 6 and 12 months
                }
                else
                {
                    interestRate = 0.07m; // 7% interest for terms longer than 12 months
                }
            }
            else if (currencyType == CurrencyType.EUR)
            {
                if (termInMonths <= 6)
                {
                    interestRate = 0.02m; // 2% interest for less than 6 months
                }
                else if (termInMonths <= 12)
                {
                    interestRate = 0.04m; // 4% interest for terms between 6 and 12 months
                }
                else
                {
                    interestRate = 0.06m; // 6% interest for terms longer than 12 months
                }
            }
            else
            {
                throw new InvalidOperationException("Currency type not supported for FD interest rate.");
            }

            return interestRate;
        }
    }

}
