using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using Microsoft.AspNetCore.Http;

namespace CAMS.Core.PresentationModels.DTOs.Client
{
    public class UpdateClientDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string SSN { get; set; } = null!;
        public IFormFile ImagePath { get; set; } = null!;
        public Address Address { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string JobTitle { get; set; } = null!;
        public decimal MonthlyIncome { get; set; }
        public decimal FinancialSource { get; set; }
    }
}
