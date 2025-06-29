using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateFixedDepositDto
    {
        public CurrencyType CurrencyType { get; set; }
        public decimal DepositAmount { get; set; }
        public int TermInMonths { get; set; }    // 3 = 3 أشهر
    }
}

