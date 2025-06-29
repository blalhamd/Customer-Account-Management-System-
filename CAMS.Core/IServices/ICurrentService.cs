using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;

namespace CAMS.Core.IServices
{
    public interface ICurrentService
    {
        Task<bool> OpenCurrentAsync(string clientId, CreateCurrentDto dto, CancellationToken ct = default);
        Task<PaginedResponse<IEnumerable<CurrentViewModel>>> GetCurrentsAsync(CurrentQuery query, CancellationToken ct = default);
        Task<CurrentViewModel> GetCurrentAsync(string currentId, CancellationToken ct = default);
    }
}
