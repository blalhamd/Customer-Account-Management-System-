namespace CAMS.Domains.Entities
{
    public class Saving : Account
    {
        public decimal InterestRate { get; set; }
        public bool HasOverdraft { get; set; }
        public int WithdrawalLimit { get; set; } // will minuse 1 with each withdrawal transaction
        public int DurationInMonthes { get; set; }
        public bool CanWithdraw { get; set; } //will be true when DurationInMonths <= 0;
    }
}

