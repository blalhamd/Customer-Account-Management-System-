using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Core.PresentationModels.ViewModels.User;

namespace CAMS.Core.IServices
{
    // ---------- 1. Identity / User Access ----------
    public interface IUserService
    {
        Task<bool> CreateAdminAsync(CreateUserDto dto);
        Task<PaginedResponse<IEnumerable<UserViewModel>>> GetUsersAsync(UserQuery userQuery);
        Task<UserViewModel?> GetByIdAsync(string userId);

        Task UpdateUserAsync(string userId, UpdateUserDto dto);
        Task DisableUserAsync(string userId);
        Task EnableUserAsync(string userId);

        Task AssignRolesAsync(string userId, IEnumerable<string> roleIds);
        Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<string> permissions);

        Task<IReadOnlyList<AuditEntryDto>> GetUserAuditTrailAsync(string userId, CancellationToken ct = default);
    }
}
