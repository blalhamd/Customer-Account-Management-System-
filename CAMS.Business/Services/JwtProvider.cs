using CAMS.Core.Constants;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.ViewModels.Auth;
using CAMS.Domains.Entities.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JsonClaimValueTypes = Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes;

namespace CAMS.Business.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtSetting _jwtSetting;

        public JwtProvider(IOptionsSnapshot<JwtSetting> jwtSetting)
        {
            _jwtSetting = jwtSetting.Value;
        }

        public JwtProviderResponse GenerateToken(AppUser applicationUser, IEnumerable<string> roles)
        {
            var descriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Expires = DateTime.Now.AddMinutes(_jwtSetting.LifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, applicationUser.UserName!),
                    new Claim(ClaimTypes.Email, applicationUser.Email!),
                    new Claim(nameof(roles), System.Text.Json.JsonSerializer.Serialize(roles),JsonClaimValueTypes.JsonArray),
                })
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var createToken = tokenHandler.CreateToken(descriptor);
            var token = tokenHandler.WriteToken(createToken);

            return new JwtProviderResponse()
            {
                Token = token,
                ExpireIn = _jwtSetting.LifeTime
            };
        }

        public string? ValidateToken(string Token)
        {
            var handler = new JwtSecurityTokenHandler();
            var SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));

            try
            {
                handler.ValidateToken(Token, new TokenValidationParameters
                {
                    IssuerSigningKey = SymmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "nameid"
                },
                out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = jwtToken.Claims.First(claim => claim.Type == "nameid").Value;

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}
