using CAMS.Domains.Entities.Base;
using CAMS.Domains.Enums;

namespace CAMS.Domains.Entities
{
    public class Account : BaseEntity<string>
    {
        public string AccountNumber { get; set; } = null!;  // must be Unique
        public string ClientId { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public CurrencyType CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; } = null!;
        public AccountStatus AccountStatus { get; set; }
        public bool IsSigned { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

