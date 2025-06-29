using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CAMS.Infrastructure.Seeding
{
    public static class SeedIdentityData // this class need to optimize in distribue the permissions
    {
        public static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole> roleManager)
        {
            // Define roles
            var roles = new List<string> { DefaultRole.Admin };

            // Ensure roles exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Assign permissions to each role
            var rolePermissions = new Dictionary<string, List<string>>
            {
              {
                DefaultRole.Admin,Permissions.GetPermissions().ToList()
              }
            };

            // Assign permissions to roles
            foreach (var rolePermission in rolePermissions)
            {
                var roleName = rolePermission.Key;
                var permissions = rolePermission.Value;

                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    foreach (var permission in permissions)
                    {
                        if (!await roleManager.RoleExistsAsync(role.Name!))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role.Name!));
                        }

                        // Check if the role already has the claim
                        var existingClaims = await roleManager.GetClaimsAsync(role);
                        if (!existingClaims.Any(c => c.Type == Permissions.Type && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(role, new Claim(Permissions.Type, permission));
                        }
                    }
                }
            }
        }
    }

}
