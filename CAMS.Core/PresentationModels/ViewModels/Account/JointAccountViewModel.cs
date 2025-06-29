using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.ViewModels.Account
{
    public class JointAccountViewModel
    {
        public string Id { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;  
        public string ClientId { get; set; } = null!;
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; } = null!;
        public AccountStatus AccountStatus { get; set; }
        public bool IsSigned { get; set; }
        public string? SecondaryClientId { get; set; }
        public bool IsEqualAccess { get; set; }
    }
}
