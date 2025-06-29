using System.Runtime.Serialization;

namespace CAMS.Domains.Enums
{
    public enum AccountStatus
    {
        [EnumMember(Value = "Active")]
        Active,

        [EnumMember(Value = "InActive")]
        InActive,

        [EnumMember(Value = "Closed")]
        Closed,

        [EnumMember(Value = "Suspended")]
        Suspended,

        [EnumMember(Value = "Pending")]
        Pending
    }
}
