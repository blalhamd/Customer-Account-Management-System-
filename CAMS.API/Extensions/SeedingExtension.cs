using CAMS.Domains.Entities.Identity;
using CAMS.Infrastructure.Data;
using CAMS.Infrastructure.Data.context;
using CAMS.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;

namespace CAMS.API.Extensions
{
    public static class SeedingExtension
    {
        public async static Task Seeding(this IServiceCollection services)
        {

            /*

             * Explain that Code in very simple

              Scope is like a copy or temporary workspace that holds services from the main container for a short period.
              It's like a room where you can use services without affecting others.

              Service Provider is like the security guard who lets you access those services inside the scope.
              You ask it for the services you need (like AppDbContext), and it gives you access.

            */

            using var scope = services.BuildServiceProvider().CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await services.Seed(userManager, roleManager, dbContext);

        }
    }
}
