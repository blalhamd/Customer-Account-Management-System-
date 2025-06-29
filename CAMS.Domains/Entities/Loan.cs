namespace CAMS.Domains.Entities
{
    public class Loan : Account
    {
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MonthlyInstallment { get; set; } // Amount to be paid each month
        public decimal RemainingBalance { get; set; } // Remaining amount to be paid
        public DateOnly DueDate { get; set; } // Next due date for installment
        public bool IsApproved { get; set; }
    }
}

