using AutoMapper;
using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Generic;
using CAMS.Core.IServices;
using CAMS.Core.IServices.Email;
using CAMS.Core.IServices.image;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Core.PresentationModels.ViewModels.User;
using CAMS.Domains.Entities;
using CAMS.Domains.Entities.Identity;
using CAMS.Domains.Enums;
using CAMS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CAMS.Business.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IGenericRepositoryAsync<AuditEntry, Guid> _auditRepository;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        public UserService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            IGenericRepositoryAsync<AuditEntry, Guid> auditRepository,
            IImageService imageService,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _auditRepository = auditRepository;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
            
        }

        public async Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<string> permissions)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if(role is null)
            {
                _logger.LogWarning("role doesn't exist");
                throw new ItemNotFoundException("role is not exist");
            }

            var claims = await _roleManager.GetClaimsAsync(role);

            foreach(var permission in permissions)
            {
                if (claims.Contains(new Claim("Permission", permission)))
                    continue;

                var result = await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));

                if(!result.Succeeded)
                    throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");
            }
        }

        public async Task AssignRolesAsync(string userId, IEnumerable<string> roleIds)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("This user is not found");
                throw new ItemNotFoundException("This user is not found");
            } 

            foreach (var roleId in roleIds)
            {
                var roleName = await _roleManager.Roles.Where(r=> r.Id == roleId).Select(x=> x.Name).FirstOrDefaultAsync();

                if (roleName is null)
                    throw new ItemNotFoundException($"Role with Id {roleId} is not exist");

                var isInRole = await _userManager.IsInRoleAsync(user, roleName!);

                if (isInRole)
                    continue;

                var result = await _userManager.AddToRoleAsync(user, roleName!);

                if(!result.Succeeded)
                    throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");
            }

            await _signInManager.RefreshSignInAsync(user); // to get on full roles
        }

        public async Task<bool> CreateAdminAsync(CreateUserDto dto)
        {
            var isExist = await _userManager.Users.AnyAsync(x => x.Email == dto.Email);

            if (isExist)
                throw new ItemAlreadyExistException("this email is in use");

            var admin = new AppUser
            {
                Email = dto.Email,
                UserName = dto.Email.Split('@')[0],
                EmailConfirmed = true,
                NormalizedEmail = dto.Email.ToUpper(),
                NormalizedUserName = dto.Email.Split('@')[0].ToUpper(),
                CreatedAt = DateTimeOffset.Now,
                IsDeleted = false,
            };

            var result = await _userManager.CreateAsync(admin, dto.Password);

            if(!result.Succeeded)
                throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");

            var assignToAdminRole = await _userManager.AddToRoleAsync(admin, "Admin");

            if(!assignToAdminRole.Succeeded)
                throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");

            return true;
        }


        public async Task DisableUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("This user is not found");
                throw new ItemNotFoundException("This user is not found");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.Now.AddYears(int.MaxValue);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");
        }

        public async Task EnableUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("This user is not found");
                throw new ItemNotFoundException("This user is not found");
            }

            user.LockoutEnabled = false;
            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");
        }

        public async Task<UserViewModel?> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("This user is not found");
                throw new ItemNotFoundException("This user is not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserViewModel
            {
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Roles = roles.ToList() 
            };
        }

        public async Task<IReadOnlyList<AuditEntryDto>> GetUserAuditTrailAsync(string userId, CancellationToken ct = default)
        {
            var query = await _auditRepository
                            .FirstOrDefaultAsync(x => x.ActorUserId == userId || x.TargetUserId == userId, null!, ct);

            if(query == null)
                return new List<AuditEntryDto>();

            var response = _mapper.Map<List<AuditEntryDto>>(query);

            return response;
        }

        public async Task<PaginedResponse<IEnumerable<UserViewModel>>> GetUsersAsync(UserQuery q)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(q.SearchText))
            {
                var text = q.SearchText.ToLower();

                query = query.Where(x => x.Email!.ToLower().Contains(text) ||
                                         x.UserName!.ToLower().Contains(text) ||
                                         x.PhoneNumber!.ToLower().Contains(text));
                                        
            }


            // --- ordering (compile-time switch, no reflection) ---------------------

            query = q.SortBy switch
            {
                "Email" => q.SortDir == SortDirection.DESC ?
                                query.OrderByDescending(u => u.Email) :
                                query.OrderBy(u => u.Email),

                "UserName" => q.SortDir == SortDirection.DESC ?
                                query.OrderByDescending(u => u.UserName) :
                                query.OrderBy(u => u.UserName),

                _ => q.SortDir == SortDirection.DESC ?
                                query.OrderByDescending(u => u.CreatedAt) : // fallback
                                query.OrderBy(u => u.CreatedAt)
            };


            q.PageNumber = Math.Max(q.PageNumber, 1);

            var totalCount = await query.CountAsync();

            var users = await query
                         .Skip((q.PageNumber - 1) * q.PageSize)
                         .Take(q.PageSize)
                         .ToListAsync();


            List<UserViewModel> list = new List<UserViewModel>();

            foreach (var user in users)
            {
                if (user is not null)
                {

                    var roles = await _userManager.GetRolesAsync(user);

                    var vm = new UserViewModel
                    {
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email! ?? string.Empty,
                        PhoneNumber = user.PhoneNumber ?? string.Empty,
                        Roles = roles.ToList() ?? new List<string>()
                    };

                    list.Add(vm);
                }
            }

            return new PaginedResponse<IEnumerable<UserViewModel>>
            {
                PageSize = q.PageSize,
                PageNumber = q.PageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / q.PageSize),
                Items = list
            };
        }

        public async Task UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("This user is not found");
                throw new ItemNotFoundException("This user is not found");
            }

            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description).ToList())}");
        }

    }
}
