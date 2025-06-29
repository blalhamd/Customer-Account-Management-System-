using CAMS.API.Filters.Authentication;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
