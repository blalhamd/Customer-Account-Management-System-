using CAMS.API.Extensions;
using CAMS.API.Filters.Authentication;
using CAMS.API.Helpers;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrentsController : ControllerBase
    {
        private readonly ICurrentService _currentService;
        private readonly ILogger<CurrentsController> _logger;
        public CurrentsController(ICurrentService currentService, ILogger<CurrentsController> logger)
        {
            _currentService = currentService;
            _logger = logger;
        }

        // POST: api/currents/open
        [HttpPost("open")]
        [RequiredPermission(Permissions.Currents.OpenCurrent)]
        public async Task<IActionResult> OpenCurrentAsync([FromBody] CreateCurrentDto dto, CancellationToken ct = default)
        {
            string? clientId = Request.GetUserId();

            if (clientId is null)
                return Unauthorized("ClientId is null");

            var success = await _currentService.OpenCurrentAsync(clientId, dto, ct);
            
            return Ok(success);
        }

        // GET: api/currents
        [HttpGet]
        [RequiredPermission(Permissions.Currents.View)]
        public async Task<IActionResult> GetCurrentsAsync([FromQuery] CurrentQuery query, CancellationToken ct = default)
        {
            var currents = await _currentService.GetCurrentsAsync(query, ct);
            return Ok(currents);
        }

        // GET: api/currents/{currentId}
        [HttpGet("{currentId}")]
        [RequiredPermission(Permissions.Currents.ViewById)]
        public async Task<IActionResult> GetCurrentAsync([FromRoute] string currentId, CancellationToken ct = default)
        {
            var current = await _currentService.GetCurrentAsync(currentId, ct);

            return Ok(current);
        }
    }
}
