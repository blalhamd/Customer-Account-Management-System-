using System.ComponentModel.DataAnnotations;

namespace CAMS.Domains.Entities
{
    public class Current : Account
    {
        [Range(50, 8000)]
        public decimal MaximumWithdrawal { get; set; }
        public decimal MinimumBalance { get; set; } = 1000;
        public bool IsBusinessAccount { get; set; }
        public decimal MonthlyFee { get; set; } // by using Hangfire to discount each month.
        public int TransactionLimit { get; set; } // will minuse 1 with each transaction happen.
    }
}

