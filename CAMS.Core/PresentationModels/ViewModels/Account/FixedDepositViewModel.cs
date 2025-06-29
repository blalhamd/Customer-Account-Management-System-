using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.ViewModels.Account
{
    public class FixedDepositViewModel
    {
        public string Id { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;  // must be Unique
        public string ClientId { get; set; } = null!;
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; } = null!;
        public AccountStatus AccountStatus { get; set; }
        public bool IsSigned { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal InterestEarned { get; set; } // الفائده المكتسبه
    }
}
