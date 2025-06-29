namespace CAMS.Domains.Entities
{
    public class FixedDeposit : Account
    {
        public decimal DepositAmount { get; set; }
        public decimal InterestEarned { get; set; } // الفائده المكتسبه by hanfire DepositAmount * Rate * Term / 12
        public decimal InterestRate { get; set; }    // % سنوي أو شهري
        public int TermInMonths { get; set; }    // مدة الوديعة بالأشهر (3, 6, 12… إلخ).
        public DateOnly MaturityDate { get; set; } // موعد الاستحقاق StartDate + TermInMonths
        public bool IsMatured { get; set; } // Flag بسيط بحيث الـ job ما يعالج نفس السطر مرتين.
    }
}

/*
 لكل وديعة:

احسب الفائدة = DepositAmount * InterestRate * TermInMonths / 12.

أضف Transaction (Deposit) إلى حساب الهدف أو حساب جارى للعميل.

غيّر IsMatured = true و AccountStatus = Closed.

أرسل إشعار (Email / Push).

احفظ التغييرات مرة واحدة.
 
 */