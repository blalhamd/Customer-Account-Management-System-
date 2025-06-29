using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateJointAccountDto
    {
        public CurrencyType CurrencyType { get; set; }
        public string? SecondaryClientId { get; set; }
        public bool IsEqualAccess { get; set; }
    }
}

