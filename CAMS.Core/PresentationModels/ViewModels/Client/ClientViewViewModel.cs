using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Core.PresentationModels.DTOs.Account;

namespace CAMS.Core.PresentationModels.ViewModels.Client
{
    public class ClientViewViewModel
    {
        public string FullName { get; set; } = null!;
        public string SSN { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public Address Address { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public Gender Gender { get; set; }
        public DateOnly BirthDate { get; set; }
        public string JobTitle { get; set; } = null!;
        public decimal MonthlyIncome { get; set; }
        public decimal FinancialSource { get; set; }
        public AppUserDto User { get; set; } = null!;
        public List<AccountDto> Accounts { get; set; } = new();
    }

}
