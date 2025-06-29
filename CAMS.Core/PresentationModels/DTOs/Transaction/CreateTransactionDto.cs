using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Transaction
{
    public class CreateTransactionDto
    {
        public string AccountNumber { get; set; } = null!;
        public string? To { get; set; } 
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
