using System.Security.Claims;

namespace CAMS.API.Extensions
{
    public static class UserClaim
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
