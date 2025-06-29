using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.ViewModels.Account
{
    public class LoanViewModel
    {
        public string Id { get; set; } = null!;
        public string AccountNumber { get; set; } = null!; 
        public string ClientId { get; set; } = null!;
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; } = null!;
        public AccountStatus AccountStatus { get; set; }
        public bool IsSigned { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MonthlyInstallment { get; set; } 
        public decimal RemainingBalance { get; set; } 
        public DateOnly DueDate { get; set; } 
        public bool IsApproved { get; set; }
    }
}
