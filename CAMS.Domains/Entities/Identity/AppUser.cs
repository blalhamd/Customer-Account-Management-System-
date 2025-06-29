using CAMS.Domains.Enums;
using Microsoft.AspNetCore.Identity;

namespace CAMS.Domains.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public UserType UserType { get; set; } 
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
