using CAMS.API.Filters.Authentication;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationsController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationsController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        // POST: api/authentication/login
        [HttpPost("login")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _authService.Login(loginRequest);
            return Ok(response);
        }


        [HttpPost("request-reset")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> RequestReset([FromBody] ForgetPasswordRequest dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _authService.RequestPasswordResetAsync(dto.Email, ip);
            return Ok(new { message = "If account exists, reset code has been sent." });
        }

        [HttpPost("verify-reset-code")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyResetCodeDto dto)
        {
            var token = await _authService.VerifyResetCodeAsync(dto.Email, dto.Code);
            return Ok(new { token });
        }

        [HttpPost("reset-password")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(new { message = "Password reset successful." });
        }
    }
}
