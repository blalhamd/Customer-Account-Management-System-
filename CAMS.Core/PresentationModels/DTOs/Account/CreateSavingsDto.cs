using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateSavingsDto
    {
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public int DurationInMonthes { get; set; }
    }
}
