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
    public class FixedDepositsController : ControllerBase
    {
        private readonly IFixedDepositService _fixedDepositService;
        private ILogger<FixedDepositsController> _logger;
        public FixedDepositsController(IFixedDepositService fixedDepositService, ILogger<FixedDepositsController> logger)
        {
            _fixedDepositService = fixedDepositService;
            _logger = logger;
        }

        // POST: api/fixeddeposits/open
        // ClientId is extracted from JWT token claims
        [HttpPost("open")]
        [RequiredPermission(Permissions.FixedDeposits.OpenFixedDeposit)]
        public async Task<IActionResult> OpenFixedDepositAsync([FromBody] CreateFixedDepositDto dto, CancellationToken ct)
        {
            string? clientId = Request.GetUserId();

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized("Client ID not found in token.");

            var success = await _fixedDepositService.OpenFixedDepositAsync(clientId, dto, ct);

            return Ok(success); 
        }

        // GET: api/fixeddeposits
        [HttpGet]
        [RequiredPermission(Permissions.FixedDeposits.View)]
        public async Task<IActionResult> GetFixedDepositsAsync([FromQuery] FixedDepositQuery query, CancellationToken ct)
        {
            var pagedResult = await _fixedDepositService.GetFixedDepositsAsync(query, ct);
            return Ok(pagedResult);
        }
    }
}
