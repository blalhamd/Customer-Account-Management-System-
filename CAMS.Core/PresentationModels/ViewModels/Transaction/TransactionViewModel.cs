using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.ViewModels.Transaction
{
    public class TransactionViewModel
    {
        public string Id { get; set; } = null!;
        public string AccountId { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }

        public decimal Total { get; set; }
        //public decimal Total => Amount - Discount;
    }
}
