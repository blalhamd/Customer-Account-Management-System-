using System.Runtime.Serialization;

namespace CAMS.Domains.Enums
{
    public enum CurrencyType
    {
        [EnumMember(Value = "United States Dollar")]
        USD,

        [EnumMember(Value = "Euro")]
        EUR,

        [EnumMember(Value = "British Pound")]
        GBP,

        [EnumMember(Value = "Japanese Yen")]
        JPY,

        [EnumMember(Value = "Canadian Dollar")]
        CAD
    }
}
