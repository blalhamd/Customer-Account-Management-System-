using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateCurrentDto
    {
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public bool IsBusinessAccount { get; set; }
    }
}
