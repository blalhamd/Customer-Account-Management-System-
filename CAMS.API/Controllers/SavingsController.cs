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
    public class SavingsController : ControllerBase
    {
        private readonly ISavingsService _savingsService;

        public SavingsController(ISavingsService savingsService)
        {
            _savingsService = savingsService;
        }

        // POST: api/savings/open
        [HttpPost("open")]
        [RequiredPermission(Permissions.Savings.Open)]
        public async Task<IActionResult> OpenSavingsAsync([FromBody] CreateSavingsDto dto, CancellationToken ct)
        {
            string? clientId = Request.GetUserId();

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized("Client ID not found in token.");

            var success = await _savingsService.OpenSavingsAsync(clientId, dto, ct);

            if (!success)
                return BadRequest("Failed to open savings account.");

            return Accepted();
        }

        // PATCH: api/savings/{savingId}/overdraft
        [HttpPut("{savingId}/overdraft")]
        [RequiredPermission(Permissions.Savings.EnableOverdraft)]
        public async Task<IActionResult> EnableOverdraftAsync(
            [FromRoute] string savingId,
            [FromQuery] bool enable,
            CancellationToken ct)
        {
            await _savingsService.EnableOverdraftAsync(savingId, enable, ct);
            return NoContent();
        }

        // GET: api/savings
        [HttpGet]
        [RequiredPermission(Permissions.Savings.View)]
        public async Task<IActionResult> GetSavingsAccountsAsync([FromQuery] SavingsQuery query, CancellationToken ct)
        {
            var savings = await _savingsService.GetSavingsAccountsAsync(query, ct);
            return Ok(savings);
        }

        // GET: api/savings/{savingId}
        [HttpGet("{savingId}")]
        [RequiredPermission(Permissions.Savings.ViewById)]
        public async Task<IActionResult> GetSavingAccountAsync([FromRoute] string savingId, CancellationToken ct)
        {
            var saving = await _savingsService.GetSavingAccountAsync(savingId, ct);

            return Ok(saving);
        }
    }
}
