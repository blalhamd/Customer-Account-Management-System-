using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Core.PresentationModels.ViewModels.Client;

namespace CAMS.Core.IServices
{
    // ---------- 2. Client Profile ----------
    public interface IClientService
    {
        Task<bool> RegisterClientAsync(CreateClientDto dto, CancellationToken ct = default);
        Task<ClientViewViewModel?> GetClientByIdAsync(string clientId);
        Task<PaginedResponse<IEnumerable<ClientViewModel>>> GetClientsAsync(ClientQuery query);
        Task UpdateClientAsync(string clientId, UpdateClientDto dto, CancellationToken ct = default);
        Task SoftDeleteClientAsync(string clientId, CancellationToken ct = default);
        Task RestoreClientAsync(string clientId);
    }
}
