using CAMS.Domains.Entities.Base;
using CAMS.Domains.Entities.Identity;
using CAMS.Domains.Enums;

namespace CAMS.Domains.Entities
{
    public class Client : BaseEntity<string>
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
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public List<Account> Accounts { get; set; } = new();
    }
}

