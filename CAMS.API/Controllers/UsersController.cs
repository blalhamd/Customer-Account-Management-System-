using CAMS.API.Filters.Authentication;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/users/admin
        [HttpPost("admin")]
        [RequiredPermission(Permissions.Users.CreateAdmin)]
        public async Task<IActionResult> CreateAdminAsync([FromBody] CreateUserDto dto)
        {
            var success = await _userService.CreateAdminAsync(dto);
            return Ok(success);
        }

        // GET: api/users
        [HttpGet]
        [RequiredPermission(Permissions.Users.View)]
        public async Task<IActionResult> GetUsersAsync([FromQuery] UserQuery userQuery)
        {
            var users = await _userService.GetUsersAsync(userQuery);
            return Ok(users);
        }

        // GET: api/users/{userId}
        [HttpGet("{userId}")]
        [RequiredPermission(Permissions.Users.ViewById)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string userId, CancellationToken ct)
        {
            var user = await _userService.GetByIdAsync(userId);
            return Ok(user);
        }

        // PUT: api/users/{userId}
        [HttpPut("{userId}")]
        [RequiredPermission(Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateUserAsync(
            [FromRoute] string userId,
            [FromBody] UpdateUserDto dto,
            CancellationToken ct)
        {
            await _userService.UpdateUserAsync(userId, dto);
            return NoContent();
        }

        // POST: api/users/{userId}/disable
        [HttpPost("{userId}/disable")]
        [RequiredPermission(Permissions.Users.Disable)]
        public async Task<IActionResult> DisableUserAsync([FromRoute] string userId, CancellationToken ct)
        {
            await _userService.DisableUserAsync(userId);
            return NoContent();
        }

        // POST: api/users/{userId}/enable
        [HttpPost("{userId}/enable")]
        [RequiredPermission(Permissions.Users.Enable)]
        public async Task<IActionResult> EnableUserAsync([FromRoute] string userId, CancellationToken ct)
        {
            await _userService.EnableUserAsync(userId);
            return NoContent();
        }

        // POST: api/users/{userId}/roles
        [HttpPost("{userId}/roles")]
        [RequiredPermission(Permissions.Users.AssignRoles)]
        public async Task<IActionResult> AssignRolesAsync(
            [FromRoute] string userId,
            [FromBody] IEnumerable<string> roleIds,
            CancellationToken ct)
        {
            await _userService.AssignRolesAsync(userId, roleIds);
            return NoContent();
        }

        // POST: api/users/roles/{roleId}/permissions
        [HttpPost("roles/{roleId}/permissions")]
        [RequiredPermission(Permissions.Roles.AssignPermissionsToRole)]
        public async Task<IActionResult> AssignPermissionsToRoleAsync(
            [FromRoute] string roleId,
            [FromBody] IEnumerable<string> permissions,
            CancellationToken ct)
        {
            await _userService.AssignPermissionsToRoleAsync(roleId, permissions);
            return NoContent();
        }

        // GET: api/users/{userId}/audit-trail
        [HttpGet("{userId}/audit-trail")]
        [RequiredPermission(Permissions.Users.ViewAuditTrail)]
        public async Task<IActionResult> GetUserAuditTrailAsync([FromRoute] string userId, CancellationToken ct)
        {
            var auditTrail = await _userService.GetUserAuditTrailAsync(userId);
            return Ok(auditTrail);
        }
    }
}
