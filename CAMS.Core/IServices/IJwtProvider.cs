using CAMS.Core.PresentationModels.ViewModels.Auth;
using CAMS.Domains.Entities.Identity;

namespace CAMS.Core.IServices
{
    public interface IJwtProvider
    {
        JwtProviderResponse GenerateToken(AppUser applicationUser, IEnumerable<string> roles); // will need user for claims(NameIdentifier,Email,GivenName,Name) that exist in Subject that exist in SecurityTokenDescriptor
        string? ValidateToken(string jwtToken);
    }
}
