using System.Runtime.Serialization;

namespace CAMS.Domains.Enums
{
    public enum TransactionType
    {

        [EnumMember(Value = "Deposit")]
        Deposit,

        [EnumMember(Value = "Withdrawal")]
        Withdrawal,

        [EnumMember(Value = "Transfer")]
        Transfer,

        [EnumMember(Value = "Payment")]
        Payment,

        [EnumMember(Value = "Query")]
        Query,

        [EnumMember(Value = "Fee")]
        Fee // رسوم
    }
}
