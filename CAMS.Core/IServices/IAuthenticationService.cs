using CAMS.Core.PresentationModels.DTOs.Auth;
using CAMS.Core.PresentationModels.ViewModels.Auth;

namespace CAMS.Core.IServices
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);

        // Reset
        Task RequestPasswordResetAsync(string email, string ip);
        Task<string> VerifyResetCodeAsync(string email, string code);
        Task ResetPasswordAsync(string token, string newPassword);
    }
}
