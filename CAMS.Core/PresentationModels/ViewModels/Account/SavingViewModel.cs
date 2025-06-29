using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.ViewModels.Account
{
    public class SavingViewModel
    {
        public string Id { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;  
        public string ClientId { get; set; } = null!;
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; } = null!;
        public AccountStatus AccountStatus { get; set; }
        public bool IsSigned { get; set; }
        public decimal InterestRate { get; set; }
        public bool HasOverdraft { get; set; }
        public int WithdrawalLimit { get; set; } 
        public int DurationInMonthes { get; set; }
        public bool CanWithdraw { get; set; } 
    }
}
