using AutoMapper;
using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Generic;
using CAMS.Core.IRepositories.Non_Generic;
using CAMS.Core.IServices;
using CAMS.Core.IServices.Email;
using CAMS.Core.IUnit;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.ViewModels.Account;
using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using CAMS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CAMS.Business.Services
{
    public class CurrentService : ICurrentService
    {
        private readonly IClientRepositoryAsync _clientRepo;
        private readonly IGenericRepositoryAsync<Current,string> _currentRepo;
        private readonly IAccountNumberGeneratorService _generator;
        private readonly ILoanPricingService _pricing;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ILogger<CurrentService> _log;
        private readonly IEmailBodyBuilder _emailBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public CurrentService(IClientRepositoryAsync clientRepo, IGenericRepositoryAsync<Current, string> currentRepo, IAccountNumberGeneratorService generator, IUnitOfWorkAsync unitOfWork, ILogger<CurrentService> log, IEmailBodyBuilder emailBuilder, IEmailSender emailSender, IMapper mapper, ILoanPricingService pricing)
        {
            _clientRepo = clientRepo;
            _currentRepo = currentRepo;
            _generator = generator;
            _unitOfWork = unitOfWork;
            _log = log;
            _emailBuilder = emailBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
            _pricing = pricing;
        }

        public async Task<CurrentViewModel> GetCurrentAsync(string currentId, CancellationToken ct = default)
        {
            var current = await _currentRepo.FirstOrDefaultAsync(c => c.Id == currentId, null!, ct);

            if (current == null)
            {
                _log.LogWarning($"current account with ID {currentId} not found");
                throw new ItemNotFoundException("current account not found");
            }

            var currentVM = _mapper.Map<CurrentViewModel>(current);

            return currentVM;
        }

        public async Task<PaginedResponse<IEnumerable<CurrentViewModel>>> GetCurrentsAsync(CurrentQuery q, CancellationToken ct = default)
        {
            Expression<Func<Current,bool>>? predicate = null;

            if (!string.IsNullOrEmpty(q.SearchBy))
            {
                var searchText = q.SearchBy.ToLower();
                predicate = (c) => c.AccountNumber.ToLower().Contains(searchText) ||
                                   c.ClientId.ToLower().Contains(searchText) ||
                                   c.Id.ToLower().Contains(searchText) ||
                                   c.AccountStatus.ToString().ToLower().Contains(searchText);
            }

            var currents = await _currentRepo.GetAllAsync(
                                   condition: predicate!,
                                   includes: null!,
                                   orderBy: x => x.CreatedAt,
                                   isAscending: false,
                                   q.PageNumber,
                                   q.PageSize,
                                   ct
                                 );

            if (currents == null)
                return new();

            var currentsVM = _mapper.Map<List<CurrentViewModel>>(currents.Items);

            return new PaginedResponse<IEnumerable<CurrentViewModel>>
            {
                Items = currentsVM,
                PageNumber = currents.PageNumber,
                PageSize = currents.PageSize,
                TotalPages = currents.TotalPages,
            };
        }

        public async Task<bool> OpenCurrentAsync(string clientId, CreateCurrentDto dto, CancellationToken ct = default)
        {
            var client = await _clientRepo.FirstOrDefaultAsync(
                              c => c.UserId == clientId,
                              includes: [q => q.Include(c => c.User)],
                              ct)
                          ?? throw new ItemNotFoundException("Client not found.");

            if (dto.Balance < 1000)
                throw new BadRequestException("Minimum Balance is 1000");

            var config = await _pricing.GetCurrentAccountConfigAsync(dto.CurrencyType, dto.IsBusinessAccount, ct);

            var acc = new Current
            {
                AccountNumber = await _generator.GenerateUniqueAccountNumberAsync(ct),
                ClientId = client.Id,
                CurrencyType = dto.CurrencyType,
                Balance = dto.Balance,
                Branch = client.Address?.City ?? string.Empty,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                IsBusinessAccount = dto.IsBusinessAccount,
                IsSigned = false,
                TransactionLimit = config.MonthlyTxnLimit,
                MaximumWithdrawal = config.MaximumWithdrawal,
                MonthlyFee = config.MonthlyFee
            };

            await _currentRepo.AddAsync(acc, ct);
            var rowsAffected = await _unitOfWork.CommitAsync(ct);
            
            if(rowsAffected <= 0)
            {
                _log.LogWarning("Failed while open current account");
                throw new InvalidOperationException("Failed while open current account");
            }

            var body = await BuildEmailBodyAsync(client.FullName, acc);
            await _emailSender.SendEmailAsync(client.User.Email!, "Your current account in Progess!", body);

            return true;
        }

        private async Task<string> BuildEmailBodyAsync(string name, Current c)
        {
            return await _emailBuilder.GenerateEmailBody(
                            "Current.html",
                            "https://bank.example.com/img/current.png",
                            $"Hi {name}, your current account is in progress!",
                            $"""
                              • Opening balance: {c.Balance:N2} {c.CurrencyType}
                              • Account no.:     {c.AccountNumber}
                              """,
                            $"https://bank.example.com/accounts/{c.Id}",
                            "View my current account");
        }
    }
}
