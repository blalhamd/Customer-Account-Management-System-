using CAMS.Domains.Entities.Base;
using CAMS.Domains.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAMS.Domains.Entities
{
    public class Transaction : BaseEntity<string>
    {
        public string AccountId { get; set; } = null!;
        public Account Account { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }

        [NotMapped]
        public decimal Total => Amount + Discount;
    }
}

