using AutoMapper;
using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Generic;
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
    public class AccountService : IAccountService
    {
        private readonly IGenericRepositoryAsync<Account, string> _accountRepo;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public AccountService(IGenericRepositoryAsync<Account, string> accountRepo, IUnitOfWorkAsync unitOfWork, ILogger<AccountService> logger, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IMapper mapper)
        {
            _accountRepo = accountRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        public async Task ChangeAccountStatusAsync(string accountId, AccountStatus newStatus, CancellationToken ct = default)
        {
            var account = await _accountRepo
                .FirstOrDefaultAsync(x => x.Id == accountId, null!, ct)??
                throw new ItemNotFoundException("account not found");

            account.AccountStatus = newStatus;

            await _accountRepo.UpdateAsync(account);
            await CommitOrThrow("Change account status", ct);
        }

        public async Task CloseAccountAsync(string accountId, CancellationToken ct = default)
        {
            var account = await _accountRepo
                .FirstOrDefaultAsync(x => x.Id == accountId,
                [q => q.Include(x => x.Client).ThenInclude(x => x.User)],
                ct) ??
                throw new ItemNotFoundException("account not found");

            account.AccountStatus = AccountStatus.Closed;

            await _accountRepo.UpdateAsync(account);
            await CommitOrThrow("Close account", ct);

            var body = await BuildEmailBodyAsync(account.Client.FullName, account);
            await _emailSender.SendEmailAsync(account.Client.User.Email!,"Your account closed", body);
        }

        public async Task FlagAccountSignedAsync(string accountId, CancellationToken ct = default)
        {
            var account = await _accountRepo
                           .FirstOrDefaultAsync(x => x.Id == accountId, null!, ct) ??
                           throw new ItemNotFoundException($"Account not found.");

            if (account.IsSigned)
                throw new BadRequestException("account is already signed");

            account.IsSigned = true;

            await _accountRepo.UpdateAsync(account);
            await CommitOrThrow("Signed account", ct);
        }

        public async Task<AccountViewViewModel?> GetAccountByIdAsync(string accountId, CancellationToken ct = default)
        {
            var account = await _accountRepo
                           .FirstOrDefaultAsync(x => x.Id == accountId, [q => q.Include(x => x.Transactions)], ct) ??
                           throw new ItemNotFoundException("account not found");

            var accountVVM = _mapper.Map<AccountViewViewModel>(account);

            return accountVVM;
        }

        public async Task<PaginedResponse<IEnumerable<AccountViewModel>>> GetAccountsForClientAsync(string clientId, AccountQuery q, CancellationToken ct = default)
        {
            Expression<Func<Account,bool>>? condition = (a) => a.ClientId == clientId;

            if (!string.IsNullOrEmpty(q.SearchBy))
            {
                var text = q.SearchBy.ToLower();
                condition = (a) => a.ClientId == clientId && 
                                  (a.AccountNumber.ToLower().Contains(text) ||
                                   a.AccountStatus.ToString().ToLower().Contains(text));
            }


            var accounts = await _accountRepo
                          .GetAllAsync(
                condition!,
                includes: null!,
                orderBy: x => x.CreatedAt,
                isAscending: false,
                pageNumber: q.PageNumber,
                pageSize: q.PageSize,
                ct) ?? new();

            var accountsVM = _mapper.Map<List<AccountViewModel>>(accounts.Items);

            return new PaginedResponse<IEnumerable<AccountViewModel>>
            {
                Items = accountsVM,
                PageSize = q.PageSize,
                PageNumber = q.PageNumber,
                TotalPages = accounts.TotalPages,
            };
        }

        public async Task ResetMonthlyLimitsAsync(CancellationToken ct = default)
        {
            var activeAccounts = await _accountRepo.GetAllAsync(
                a => a.AccountStatus == AccountStatus.Active && (a is Current || a is Saving),
                includes: null!,
                orderBy: x => x.CreatedAt,
                isAscending: false,
                pageNumber: 1,
                pageSize: int.MaxValue,
                ct);

            if (!activeAccounts.Items.Any())
            {
                _logger.LogInformation("No active accounts found for monthly limits reset.");
                return;
            }

            foreach (var account in activeAccounts.Items)
            {
                switch (account)
                {
                    case Current current:
                        await ResetCurrentAccountLimitsAsync(current);
                        break;
                    case Saving saving:
                        await ResetSavingAccountLimitsAsync(saving);
                        break;
                    default:
                        _logger.LogWarning("Account type not supported for reset: {AccountType}", account.GetType());
                        break;
                }
            }

            await _accountRepo.UpdateRangeAsync(activeAccounts.Items);
            await CommitOrThrow("Reset Monthly Limits For Current and Saving accounts", ct);
        }

        private async Task ResetCurrentAccountLimitsAsync(Current current)
        {
            _logger.LogInformation("Resetting monthly limits for Current account ID {AccountId}", current.Id);

            current.TransactionLimit = 8;
            current.MonthlyFee = current.Balance * 0.01m;  
            current.Balance -= current.MonthlyFee;
        }

        private async Task ResetSavingAccountLimitsAsync(Saving saving)
        {
            _logger.LogInformation("Resetting monthly limits for Saving account ID {AccountId}", saving.Id);

            // Reset withdrawal limits for active Saving accounts
            saving.WithdrawalLimit = 8;
        }


        private async Task CommitOrThrow(string action, CancellationToken ct = default)
        {
            if (await _unitOfWork.CommitAsync(ct) <= 0)
                throw new InvalidOperationException($"Failed while {action}");
        }

        private async Task<string> BuildEmailBodyAsync(string name, Account account)
        {
            var body = await _emailBodyBuilder.GenerateEmailBody(
                templateName: "Account.html",
                imageUrl: "https://bank.example.com/img/loan.png",  // Add more relevant images
                header: $"Hi {name}, your account is now closed!",
                textBody: GenerateAccountTextBody(account),
                link: $"https://bank.example.com/accounts/{account.Id}",
                linkTitle: "View my account");

            return body;
        }

        private string GenerateAccountTextBody(Account account)
        {
            return $"""
        • Balance: {account.Balance:N2} {account.CurrencyType}
        • Account Number: {account.AccountNumber}
        • Account Status: {account.AccountStatus}
        """;
        }

    }
}
