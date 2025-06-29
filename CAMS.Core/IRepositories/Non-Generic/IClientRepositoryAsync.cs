using CAMS.Core.IRepositories.Generic;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Domains.Entities;

namespace CAMS.Core.IRepositories.Non_Generic
{
    public interface IClientRepositoryAsync : IGenericRepositoryAsync<Client,string>
    {
        Task<ClientDto?> GetClientByIdAsync(string clientId);
        Task<Client> GetSoftDeleteClientAsync(string clientId);
    }
}
