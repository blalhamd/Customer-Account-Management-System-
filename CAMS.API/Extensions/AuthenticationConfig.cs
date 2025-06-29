using CAMS.Core.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CAMS.API.Extensions
{
    public static class AuthenticationConfig
    {

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtOptions = configuration.GetSection("Jwt").Get<JwtSetting>();

            services.AddSingleton(JwtOptions!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.SaveToken = true;
                
                // here will validate parameters of Token.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "nameid",
                    ValidateIssuer = true,
                    ValidIssuer = JwtOptions!.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.Key))
                };
            });
           

            return services;
        }
    }
}
