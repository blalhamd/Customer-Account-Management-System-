using Asp.Versioning;
using CAMS.API.Filters.Authentication;
using CAMS.API.MiddleWares.Models;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    //[Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // POST: api/clients
        [HttpPost]
        [RequiredPermission(Permissions.Clients.RegisterClient)]
        [MapToApiVersion("1.0")]
        [EndpointName("RegisterClient")]
        [EndpointDescription("Register a new client in (CAM) System and return boolean response.")]
        [EndpointSummary("Register a new client in (CAM) System.")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [ProducesResponseType<CustomResponse>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<CustomResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<CustomResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<CustomResponse>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<CustomResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterClient([FromForm] CreateClientDto dto)
        {
            return Ok(await _clientService.RegisterClientAsync(dto));
        }


        // GET: api/clients/{clientId}
        [HttpGet("{clientId}")]
        [RequiredPermission(Permissions.Clients.ViewById)]
        public async Task<IActionResult> GetClientById([FromRoute] string clientId)
        {
            return Ok(await _clientService.GetClientByIdAsync(clientId));
        }

        // GET: api/clients
        [HttpGet]
        [RequiredPermission(Permissions.Clients.View)]
        public async Task<IActionResult> GetClients([FromQuery] ClientQuery query)
        {
            var clients = await _clientService.GetClientsAsync(query);
            return Ok(clients);
        }

        // PUT: api/clients/{clientId}
        [HttpPut("{clientId}")]
        [RequiredPermission(Permissions.Clients.Edit)]
        public async Task<IActionResult> UpdateClient([FromRoute] string clientId, [FromForm] UpdateClientDto dto)
        {
            await _clientService.UpdateClientAsync(clientId, dto);
            return NoContent();
        }

        // DELETE: api/clients/{clientId}
        [HttpDelete("{clientId}")]
        [RequiredPermission(Permissions.Clients.Soft)]
        public async Task<IActionResult> SoftDeleteClient([FromRoute] string clientId)
        {
            await _clientService.SoftDeleteClientAsync(clientId);
            return NoContent();
        }

        // POST: api/clients/{clientId}/restore
        [HttpPost("{clientId}/restore")]
        [RequiredPermission(Permissions.Clients.Restore)]
        public async Task<IActionResult> RestoreClient([FromRoute] string clientId)
        {
            await _clientService.RestoreClientAsync(clientId);
            return NoContent();
        }
    }
}
