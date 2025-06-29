using CAMS.Core.IRepositories.Generic;
using CAMS.Core.IServices;
using CAMS.Core.IUnit;
using CAMS.Core.PresentationModels.ViewModels.Auth;
using CAMS.Domains.Entities;
using CAMS.Domains.Entities.Identity;
using CAMS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoginRequest = CAMS.Core.PresentationModels.DTOs.Auth.LoginRequest;

namespace CAMS.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _config;
        private readonly IGenericRepositoryAsync<PasswordResetCode,Guid> _passwordResetCodeRepository;

        public AuthenticationService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, IJwtProvider jwtProvider, IGenericRepositoryAsync<PasswordResetCode, Guid> passwordResetCodeRepository, IUnitOfWorkAsync unitOfWork, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _jwtProvider = jwtProvider;
            _unitOfWork = unitOfWork;
            _config = config;
            _passwordResetCodeRepository = passwordResetCodeRepository;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new BadRequestException("Invalid UserName or Password");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, true);

            if (!result.Succeeded)
            {
                throw result.IsNotAllowed ? new BadRequestException("You must confirm your email.")
                     : result.IsLockedOut ? new BadRequestException("You are Locked Out")
                     : new BadRequestException("Invalid Email Or Password");
            }

            var roles = await GetRoles(user);
            var jwtProviderResponse = _jwtProvider.GenerateToken(user, roles);

            var response = GetLoginResponse(user, jwtProviderResponse);

            return response ?? throw new BadRequestException("Invalid UserName or Password");
        }


        public async Task<bool> ValidateEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user is null ? false : true;
        }


        private LoginResponse GetLoginResponse(AppUser user, JwtProviderResponse response)
        {

            return new LoginResponse()
            {
                Token = response.Token,
                ExpireIn = response.ExpireIn * 60,
            };
        }


        private async Task<IEnumerable<string>> GetRoles(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return roles;
        }

        public async Task RequestPasswordResetAsync(string email, string ip)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return; // Silent fail for security

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var hash = BCrypt.Net.BCrypt.HashPassword(code);
            

            var resetCode = new PasswordResetCode
            {
                UserId = user.Id,
                CodeHash = hash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IPAddress = ip
            };

            await _passwordResetCodeRepository.AddAsync(resetCode);
            await _unitOfWork.CommitAsync();

            string body = $"<p>Your password reset code is <strong>{code}</strong>. It expires in 15 minutes.</p>";
            await _emailSender.SendEmailAsync(email, "Password Reset Code", body);
        }

        public async Task<string> VerifyResetCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new UnauthorizedAccessException("Invalid code");

            var recentCode = await _passwordResetCodeRepository
                .FirstOrDefaultAsync(x => x.UserId == user.Id && !x.Used && x.ExpiresAt > DateTime.UtcNow,
                                     null!, x => x.CreatedAt, isAscending: false);

            if (recentCode == null || !BCrypt.Net.BCrypt.Verify(code, recentCode.CodeHash))
                throw new UnauthorizedAccessException("Invalid or expired code");

            recentCode.Used = true;
            await _unitOfWork.CommitAsync();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("resetUserId", user.Id) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true
            }, out _);

            var userId = principal.FindFirst("resetUserId")?.Value;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("Invalid token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new UnauthorizedAccessException("Invalid token");

            var tokenReset = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, tokenReset, newPassword);

            if (!result.Succeeded)
                throw new Exception("Password reset failed");
        }
    }
}
