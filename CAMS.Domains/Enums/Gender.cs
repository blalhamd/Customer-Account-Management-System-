using System.Runtime.Serialization;

namespace CAMS.Domains.Enums
{
    public enum Gender
    {
        [EnumMember(Value = "UnKnown")]
        UnKnown,

        [EnumMember(Value = "Male")]
        Male,

        [EnumMember(Value = "Female")]
        Female
    }
}
