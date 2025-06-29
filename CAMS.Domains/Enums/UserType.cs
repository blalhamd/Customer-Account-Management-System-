using System.Runtime.Serialization;

namespace CAMS.Domains.Enums
{
    public enum UserType
    {
        [EnumMember(Value = "Admin")]
        Admin,

        [EnumMember(Value = "Client")]
        Client
    }
}
