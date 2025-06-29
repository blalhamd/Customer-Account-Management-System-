using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.Transaction
{
    public class TransactionDto
    {
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        //public decimal Total => Amount - Discount;
    }
}
