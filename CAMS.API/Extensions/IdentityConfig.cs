using CAMS.Domains.Entities.Identity;
using CAMS.Infrastructure.Data.context;
using Microsoft.AspNetCore.Identity;

namespace CAMS.API.Extensions
{
    public static class IdentityConfig
    {
        public static IServiceCollection RegisterIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.Password.RequiredLength = 8;
            })
                  .AddEntityFrameworkStores<AppDbContext>()
                  .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);


            return services;
        }
    }
}
