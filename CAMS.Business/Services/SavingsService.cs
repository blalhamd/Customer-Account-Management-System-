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
using System.Linq;
using System.Linq.Expressions;

namespace CAMS.Business.Services
{
    public class SavingsService : ISavingsService
    {
        private readonly IGenericRepositoryAsync<Saving,string> _savingRepo;
        private readonly IClientRepositoryAsync _clientRepo;
        private readonly IAccountNumberGeneratorService _generator;
        private readonly ILoanPricingService _pricing;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ILogger<SavingsService> _log;
        private readonly IEmailBodyBuilder _emailBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public SavingsService(IGenericRepositoryAsync<Saving, string> savingRepo, IClientRepositoryAsync clientRepo, IAccountNumberGeneratorService generator, IUnitOfWorkAsync unitOfWork, ILogger<SavingsService> log, IEmailBodyBuilder emailBuilder, IEmailSender emailSender, IMapper mapper, ILoanPricingService pricing)
        {
            _savingRepo = savingRepo;
            _clientRepo = clientRepo;
            _generator = generator;
            _unitOfWork = unitOfWork;
            _log = log;
            _emailBuilder = emailBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
            _pricing = pricing;
        }

        public async Task EnableOverdraftAsync(string savingsAccountId, bool enable, CancellationToken ct = default)
        {
            var savingAccount = await _savingRepo.FirstOrDefaultAsync(x => x.Id == savingsAccountId, null!, ct);

            if(savingAccount is null)
            {
                _log.LogWarning($"saving account not found with #ID {savingsAccountId}");
                throw new ItemNotFoundException("saving account not found");
            }

            if (savingAccount.HasOverdraft == enable)
                throw new BadRequestException(
                    enable ? "Overdraft already enabled." : "Overdraft already disabled.");

            savingAccount.HasOverdraft = enable;
            await _savingRepo.UpdateAsync(savingAccount);
            await CommitOrThrowAsync("toggle overdraft", ct);
        }

        public async Task<SavingViewModel> GetSavingAccountAsync(string savingId, CancellationToken ct = default)
        {
            var savingAccount = await _savingRepo.FirstOrDefaultAsync(x => x.Id == savingId, null!, ct);

            if (savingAccount is null)
            {
                _log.LogWarning($"saving account not found with #ID {savingId}");
                throw new ItemNotFoundException("saving account not found");
            }

            var savingVM = _mapper.Map<SavingViewModel>(savingAccount);

            return savingVM;
        }

        public async Task<PaginedResponse<IEnumerable<SavingViewModel>>> GetSavingsAccountsAsync(SavingsQuery q, CancellationToken ct = default)
        {
            Expression<Func<Saving, bool>> predicate = s => true;

            if (!string.IsNullOrWhiteSpace(q.SearchBy))
            {
                var t = q.SearchBy.ToLower();
                predicate = s =>
                     s.AccountNumber.ToLower().Contains(t) ||
                     s.ClientId.ToLower().Contains(t) ||
                     s.Id.ToLower().Contains(t) ||
                     s.AccountStatus.ToString().ToLower().Contains(t);
            }

            var savingsAccounts = await _savingRepo.GetAllAsync(
                                                      condition: predicate!,
                                                      includes: null!,
                                                      orderBy: x => x.CreatedAt,
                                                      isAscending: false,
                                                      q.PageNumber,
                                                      q.PageSize,
                                                      ct
                                                    );

            if (savingsAccounts is null)
                return new();

            var savingsAccountsVM = _mapper.Map<List<SavingViewModel>>(savingsAccounts.Items);

            return new PaginedResponse<IEnumerable<SavingViewModel>>
            {
                Items = savingsAccountsVM,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize,
                TotalPages = savingsAccounts.TotalPages,
            };
        }

        public async Task<bool> OpenSavingsAsync(string clientId, CreateSavingsDto dto, CancellationToken ct = default)
        {
            var client = await _clientRepo.FirstOrDefaultAsync(x => x.UserId == clientId, [q => q.Include(x => x.User)], ct);

            if(client is null)
            {
                _log.LogWarning($"client with this ID {clientId} not found");
                throw new ItemNotFoundException("client not found");
            }

            var rate = await _pricing.GetAnnualRateAsync(dto.CurrencyType, dto.Balance, ct); // reuse pricing svc

            var saving = new Saving
            {
                AccountNumber = await _generator.GenerateUniqueAccountNumberAsync(ct),
                Balance = dto.Balance,
                Branch = client.Address.City ?? string.Empty,
                AccountStatus = AccountStatus.Pending,
                CurrencyType = dto.CurrencyType,
                CreatedAt = DateTimeOffset.UtcNow,
                DurationInMonthes = dto.DurationInMonthes,
                HasOverdraft = false,
                IsSigned = false,
                WithdrawalLimit = 5,
                CanWithdraw = true,
                ClientId = client.Id,
                InterestRate = rate 
            };

            await _savingRepo.AddAsync(saving, ct);
            await CommitOrThrowAsync("Open saving account", ct);

            var body = await BuildEmailBodyAsync(client.User.Email!, saving);
            await _emailSender.SendEmailAsync(body, "Your saving request is pending", body);

            return true;
        }

        private async Task CommitOrThrowAsync(string action, CancellationToken ct = default)
        {
            if (await _unitOfWork.CommitAsync(ct) <= 0)
                throw new InvalidOperationException($"Failed while {action}");
        }

        private async Task<string> BuildEmailBodyAsync(string name, Saving s)
        {
            return await _emailBuilder.GenerateEmailBody(
                         templateName: "Saving.html",
                         imageUrl: "https://bank.example.com/img/saving.png",
                         header: $"Hi {name}, your saving account is in progress!",
                         textBody:
                             $"""
                              • Amount:   {s.Balance:N2} {s.CurrencyType}
                              • Term:     {s.DurationInMonthes} month{(s.DurationInMonthes > 1 ? "s" : "")}
                              • Rate:     {s.InterestRate:P2}
                              • Account:  {s.AccountNumber}
                              """,
                         link: $"https://bank.example.com/accounts/{s.Id}",
                         linkTitle: "View my saving account");
        }
    }
}
