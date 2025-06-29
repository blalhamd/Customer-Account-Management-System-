using CAMS.Core.IServices;
using CAMS.Domains.Entities.Identity;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Identity;

namespace CAMS.Business.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserPermissionService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<string>> GetPermissionsForUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);

            var permissions = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    if (claim.Type == Permissions.Type)
                    {
                        permissions.Add(claim.Value);
                    }
                }
            }

            return permissions.ToList();
        }
    }
}
