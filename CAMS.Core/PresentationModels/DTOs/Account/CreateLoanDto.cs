using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateLoanDto
    {
        public CurrencyType CurrencyType { get; set; }
        public decimal LoanAmount { get; set; }
    }
}
