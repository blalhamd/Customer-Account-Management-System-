using CAMS.Domains.Entities.Identity;
using CAMS.Domains.Enums;
using CAMS.Infrastructure.constants;
using CAMS.Infrastructure.Data.context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CAMS.Infrastructure.Seeding
{
    public static class Seeding
    {
        public static async Task<IServiceCollection> Seed(this IServiceCollection services, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
           
            // Seed roles
            if (!await roleManager.Roles.AnyAsync())
            {
                foreach (var role in LoadRoles())
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // Seed admin user
            if (!await userManager.Users.AnyAsync())
            {
                foreach (var user in LoadUsers())
                {
                    var existingUser = await userManager.FindByEmailAsync(user.Email!);
                    if (existingUser == null)
                    {
                        var result = await userManager.CreateAsync(user, DefaultUser.AdminPassword);
                        if (result.Succeeded)
                            await userManager.AddToRoleAsync(user, DefaultRole.Admin);
                    }
                }
            }

            // Seed role claims
            if (!await context.RoleClaims.AnyAsync())
            {
                foreach (var claim in LoadPermissions())
                {
                    await context.RoleClaims.AddAsync(claim);
                }

                await context.SaveChangesAsync();
            }

            // Optional: Seed user-role manually (if needed by external logic)
            if (!await context.UserRoles.AnyAsync())
            {
                foreach (var userRole in LoadUserRoles())
                {
                    await context.UserRoles.AddAsync(userRole);
                }

                await context.SaveChangesAsync();
            }

            return services;
        }

        private static IEnumerable<AppUser> LoadUsers()
        {
            return new List<AppUser>
        {
            new AppUser
            {
                Id = DefaultUser.AdminId,
                UserName = DefaultUser.AdminEmail.Split('@')[0],
                Email = DefaultUser.AdminEmail,
                EmailConfirmed = true,
                UserType = UserType.Admin
            }
        };
        }

        private static IEnumerable<IdentityRole> LoadRoles()
        {
            return new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = DefaultRole.AdminId,
                Name = DefaultRole.Admin,
                NormalizedName = DefaultRole.Admin.ToUpper(),
                ConcurrencyStamp = DefaultRole.AdminConcurrencyStamp
            }
        };
        }

        private static IEnumerable<IdentityUserRole<string>> LoadUserRoles()
        {
            return new List<IdentityUserRole<string>>
        {
            new IdentityUserRole<string>
            {
                RoleId = DefaultRole.AdminId,
                UserId = DefaultUser.AdminId
            }
        };
        }

        private static IEnumerable<IdentityRoleClaim<string>> LoadPermissions()
        {
            return Permissions.GetPermissions().Select(permission => new IdentityRoleClaim<string>
            {
                RoleId = DefaultRole.AdminId,
                ClaimType = Permissions.Type,
                ClaimValue = permission
            });
        }
    }


}
