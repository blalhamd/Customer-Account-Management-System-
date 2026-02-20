
using AutoMapper;
using Azure;
using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Non_Generic;
using CAMS.Core.IServices;
using CAMS.Core.IServices.Email;
using CAMS.Core.IServices.image;
using CAMS.Core.IUnit;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Core.PresentationModels.ViewModels.Client;
using CAMS.Domains.Entities;
using CAMS.Domains.Entities.Identity;
using CAMS.Domains.Enums;
using CAMS.Infrastructure.constants;
using CAMS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CAMS.Business.Services
{
    public class ClientService : IClientService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IClientRepositoryAsync _clientRepository;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        public ClientService(UserManager<AppUser> userManager, IClientRepositoryAsync clientRepository, IUnitOfWorkAsync unitOfWork, IImageService imageService, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IMapper mapper, ILogger<UserService> logger, IConfiguration config)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
            _config = config;
        }


        public async Task<ClientViewViewModel?> GetClientByIdAsync(string clientId)
        {
            var query = await _clientRepository.GetClientByIdAsync(clientId);

            if(query == null)
            {
                _logger.LogWarning("doesn't exist client with this #id {0}", clientId);
                throw new ItemNotFoundException("doesn't exist client with this id");
            }

            var response = _mapper.Map<ClientViewViewModel>(query);
            response.ImagePath = string.Concat(_config["BaseUrl"] ?? string.Empty, response.ImagePath ?? string.Empty);

            return response;
        }

        public async Task<PaginedResponse<IEnumerable<ClientViewModel>>> GetClientsAsync(ClientQuery query)
        {
            Expression<Func<Client, bool>>? filter = null;

            if (!string.IsNullOrEmpty(query.SearchBy))
            {
                var text = query.SearchBy.ToLower();

                filter = (x) => x.FullName.ToLower().Contains(text) ||
                                                                   x.SSN.ToLower() == text ||
                                                                   x.JobTitle.ToLower().Contains(text);
            }

            var clients = await _clientRepository.GetAllAsync(
                          filter?? null!,
                          includes: null!,
                          orderBy: x => x.CreatedAt,
                          isAscending: false,
                          pageNumber: query.PageNumber,
                          pageSize: query.PageSize);
            

            if (clients is null)
                return new PaginedResponse<IEnumerable<ClientViewModel>>();

            var clientsVM = _mapper.Map<List<ClientViewModel>>(clients.Items);

            var baseUrl = _config["BaseUrl"] ?? string.Empty;

            clientsVM.ForEach(x =>
            {
                x.ImagePath = string.Concat(baseUrl, x.ImagePath ?? string.Empty);
            });


            return new PaginedResponse<IEnumerable<ClientViewModel>>
            {
                Items = clientsVM,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = clients.TotalPages,
            };
        }

        public async Task<bool> RegisterClientAsync(CreateClientDto dto, CancellationToken ct = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
                    var isExist = await _userManager.Users.AnyAsync(x => x.Email == dto.Email, ct);
                    if (isExist)
                        throw new ItemAlreadyExistException("this email is in use");

                    var user = new AppUser
                    {
                        Email = dto.Email,
                        UserName = dto.Email.Split('@')[0],
                        EmailConfirmed = true,
                        NormalizedEmail = dto.Email.ToUpper(),
                        NormalizedUserName = dto.Email.Split('@')[0].ToUpper(),
                        CreatedAt = DateTimeOffset.Now,
                        IsDeleted = false,
                        UserType = UserType.Client
                    };

                    var result = await _userManager.CreateAsync(user, dto.Password);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"{string.Join(',', result.Errors.Select(x => x.Description))}");

                    // add to client role
                    var res = await _userManager.AddToRoleAsync(user, DefaultRole.Client);
                    if (!res.Succeeded)
                        throw new InvalidOperationException($"{string.Join(',', res.Errors.Select(x => x.Description))}");

                    if (!Enum.IsDefined(typeof(Gender), dto.Gender))
                        throw new BadRequestException("Gender is Invalid");

                    var newImagePath = await _imageService.UploadImageOnServer(dto.ImagePath, false, null!, ct);

                    var client = _mapper.Map<Client>(dto);
                    client.UserId = user.Id;

                    if (newImagePath != null)
                        client.ImagePath = newImagePath;

                    await _clientRepository.AddAsync(client, ct);
                    var rowsAffected = await _unitOfWork.CommitAsync(ct);

                    if (rowsAffected <= 0)
                    {
                        if (newImagePath != null)
                            await _imageService.RemoveImage(newImagePath);

                        throw new InvalidOperationException("Client Can't add");
                    }

                    // TODO: Send Email with email and password to login in CAMS system

                    var body = await BuildEmailBodyAsync(client.FullName, user.Email, user.PasswordHash!);

                    await _emailSender.SendEmailAsync(user.Email!, subject: "Welcome to CAMS – Your Account is Ready", body);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message);
                    throw;
                }
            });
        }


        public async Task RestoreClientAsync(string clientId)
        {
            var client = await _clientRepository.GetSoftDeleteClientAsync(clientId);

            if (client is null)
                throw new ItemNotFoundException("Client is not found");

            client.UndoDeleted();

            await _clientRepository.UpdateAsync(client);
            await _unitOfWork.CommitAsync();
        }


        public async Task SoftDeleteClientAsync(string clientId, CancellationToken ct = default)
        {
            var client = await _clientRepository.FirstOrDefaultAsync(x=> x.Id == clientId,null!,ct);

            if (client is null)
                throw new ItemNotFoundException("Client is not found");

            client.DoDeleted(clientId);

            await _clientRepository.UpdateAsync(client);
            var rowsAffected = await _unitOfWork.CommitAsync(ct);

            if (rowsAffected <= 0)
                throw new InvalidOperationException($"Client can't delete");
        }

        public async Task UpdateClientAsync(string clientId, UpdateClientDto dto, CancellationToken ct = default)
        {
            var client = await _clientRepository.FirstOrDefaultAsync(x => x.Id == clientId, [q => q.Include(x => x.User)], ct);

            if (client is null)
                throw new ItemNotFoundException("Client is not found");

            var oldPath = client.ImagePath;
            var newImagePath = await _imageService.UploadImageOnServer(dto.ImagePath, false, client.ImagePath);
            var map = _mapper.Map(dto,client);

            if(newImagePath != null)
               map.ImagePath = newImagePath;

            await _clientRepository.UpdateAsync(map);
            var rowsAffected = await _unitOfWork.CommitAsync(ct);

            if (rowsAffected <= 0)
            {
                if (newImagePath != null)
                    await _imageService.RemoveImage(newImagePath);

                throw new InvalidOperationException("Client Can't edit");
            }

            await _imageService.RemoveImage(oldPath);
        }

        private async Task<string> BuildEmailBodyAsync(string name, string email, string password)
        {
            return await _emailBodyBuilder.GenerateEmailBody(
                      templateName: "Client.html",
                      imageUrl: "https://yourdomain.com/assets/welcome.png", // optional
                      header: $"Hi {name},",
                      textBody: $"Your account has been created. Click the button below to access CAMS.",
                      link: "https://cams.example.com/login",
                      linkTitle: "Sign In"
            );
        }


    }
}
