using CAMS.API.Extensions;
using CAMS.API.Filters.Authentication;
using CAMS.API.Helpers;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JointAccountsController : ControllerBase
    {
        private readonly IJointAccountService _jointAccountService;

        public JointAccountsController(IJointAccountService jointAccountService)
        {
            _jointAccountService = jointAccountService;
        }

        // POST: api/jointaccounts
        // Creates a joint account for authenticated client (clientId from token)
        [HttpPost]
        [RequiredPermission(Permissions.JointAccounts.OpenJointAccount)]
        public async Task<IActionResult> CreateJointAccountAsync([FromBody] CreateJointAccountDto dto, CancellationToken ct)
        {
            string? clientId = Request.GetUserId();

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized("Client ID not found in token.");

            var success = await _jointAccountService.CreateJointAccountAsync(clientId, dto, ct);
            if (!success)
                return BadRequest("Failed to create joint account.");

            return Accepted(); // or CreatedAtAction if you want to return location URI
        }

        // POST: api/jointaccounts/{jointAccountId}/secondary-holder
        [HttpPost("{jointAccountId}/secondary-holder")]
        [RequiredPermission(Permissions.JointAccounts.AddSecondary)]
        public async Task<IActionResult> AddSecondaryHolderAsync([FromRoute] string jointAccountId, [FromBody] CreateSecondryHolderDto dto, CancellationToken ct)
        {
            await _jointAccountService.AddSecondaryHolderAsync(jointAccountId, dto, ct);
            return NoContent();
        }

        // DELETE: api/jointaccounts/{jointAccountId}/secondary-holder/{secondaryClientId}
        [HttpDelete("{jointAccountId}/secondary-holder/{secondaryClientId}")]
        [RequiredPermission(Permissions.JointAccounts.RemoveSecondary)]
        public async Task<IActionResult> RemoveSecondaryHolderAsync(
            [FromRoute] string jointAccountId,
            [FromRoute] string secondaryClientId,
            CancellationToken ct)
        {
            await _jointAccountService.RemoveSecondaryHolderAsync(jointAccountId, secondaryClientId, ct);
            return NoContent();
        }

        // GET: api/jointaccounts
        [HttpGet]
        [RequiredPermission(Permissions.JointAccounts.View)]
        public async Task<IActionResult> GetJointAccountsAsync([FromQuery] JointAccountQuery query, CancellationToken ct)
        {
            var pagedResult = await _jointAccountService.GetJointAccountsAsync(query, ct);
            return Ok(pagedResult);
        }
    }
}
