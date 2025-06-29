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
    public sealed class LoanService : ILoanService
    {
        private readonly IGenericRepositoryAsync<Loan, string> _loanRepo;
        private readonly IGenericRepositoryAsync<Account, string> _accountRepo;
        private readonly IClientRepositoryAsync _clientRepo;
        private readonly IAccountNumberGeneratorService _generator;
        private readonly IUnitOfWorkAsync _uow;
        private readonly ILoanPricingService _pricing;
        private readonly IEmailBodyBuilder _email;
        private readonly IEmailSender _mailer;
        private readonly ILogger<LoanService> _log;
        private readonly IMapper _mapper;

        public LoanService(
            IGenericRepositoryAsync<Loan, string> loanRepo,
            IGenericRepositoryAsync<Account, string> accountRepo,
            IClientRepositoryAsync clientRepo,
            IAccountNumberGeneratorService generator,
            IUnitOfWorkAsync uow,
            ILoanPricingService pricing,
            IEmailBodyBuilder email,
            IEmailSender mailer,
            ILogger<LoanService> log,
            IMapper mapper)
        {
            _loanRepo = loanRepo;
            _accountRepo = accountRepo;
            _clientRepo = clientRepo;
            _generator = generator;
            _uow = uow;
            _pricing = pricing;
            _email = email;
            _mailer = mailer;
            _log = log;
            _mapper = mapper;
        }

        /*-------------------------------------------------------*/
        /* 1)  APPLY FOR LOAN                                    */
        /*-------------------------------------------------------*/
        public async Task<bool> ApplyForLoanAsync(
            string clientId, CreateLoanDto dto, CancellationToken ct = default)
        {
            var client = await _clientRepo.FirstOrDefaultAsync(
                             c => c.UserId == clientId,
                             includes: [q => q.Include(c => c.User)],
                             ct)
                         ?? throw new ItemNotFoundException("Client not found.");

            var rate = await _pricing.GetAnnualRateAsync(dto.CurrencyType, dto.LoanAmount, ct);
            var interest = dto.LoanAmount * rate;
            var grandTotal = dto.LoanAmount + interest;
            var monthly = decimal.Round(grandTotal / 12, 2);

            var loan = new Loan
            {
                AccountNumber = await _generator.GenerateUniqueAccountNumberAsync(ct),
                ClientId = client.Id,
                CurrencyType = dto.CurrencyType,
                LoanAmount = dto.LoanAmount,
                InterestRate = rate,
                MonthlyInstallment = monthly,
                RemainingBalance = grandTotal,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                Branch = client.Address?.City ?? string.Empty,
                Balance = dto.LoanAmount,           // GL exposure
                AccountStatus = AccountStatus.Pending,
                IsApproved = false,
                IsSigned = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _loanRepo.AddAsync(loan, ct);
            await CommitOrThrowAsync("creating loan", ct);

            var html = await BuildLoanEmailAsync(client.FullName, loan);
            await _mailer.SendEmailAsync(client.User.Email!,
                                         "Your loan request is pending approval",
                                         html);

            return true;
        }

        /*-------------------------------------------------------*/
        /* 2)  APPROVE LOAN (back-office)                        */
        /*-------------------------------------------------------*/
        public async Task ApproveLoanAsync(string loanId, CancellationToken ct = default)
        {
            var loan = await _loanRepo.FirstOrDefaultAsync(l => l.Id == loanId,null!, ct)
                       ?? throw new ItemNotFoundException("Loan account not found.");

            if (loan.IsApproved)
                throw new InvalidOperationException("Loan already approved.");

            loan.IsApproved = true;
            loan.IsSigned = true;
            loan.AccountStatus = AccountStatus.Active;

            await _loanRepo.UpdateAsync(loan);
            await CommitOrThrowAsync("approving loan", ct);
        }


        /*-------------------------------------------------------*/
        /* 4)  QUERY METHODS                                     */
        /*-------------------------------------------------------*/
        public async Task<LoanViewModel?> GetLoanByIdAsync(string loanId, CancellationToken ct = default)
        {
            var loan = await _loanRepo
                         .FirstOrDefaultAsync(l => l.Id == loanId, null!, ct);

            return loan is null
                ? null
                : _mapper.Map<LoanViewModel>(loan);
        }

        public async Task<PaginedResponse<IEnumerable<LoanViewModel>>> GetLoansAsync(LoanQuery q, CancellationToken ct = default)
        {
            Expression<Func<Loan, bool>>? predicate = null;

            if (!string.IsNullOrWhiteSpace(q.SearchBy))
            {
                var t = q.SearchBy.ToLower();
                predicate = l =>
                    l.AccountNumber.ToLower().Contains(t) ||
                    l.ClientId.ToLower().Contains(t) ||
                    l.Id.ToLower().Contains(t);
            }

            var page = await _loanRepo.GetAllAsync(
                           predicate!,
                           orderBy: l => l.CreatedAt,
                           isAscending: true,
                           pageNumber: q.PageNumber,
                           pageSize: q.PageSize,
                           cancellationToken: ct);

            var vms = _mapper.Map<List<LoanViewModel>>(page.Items);

            return new PaginedResponse<IEnumerable<LoanViewModel>>
            {
                Items = vms,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize,
                TotalPages = page.TotalPages
            };
        }

        /* --------------- 1. manual payment ---------------- */
        public async Task<bool> MakeInstallmentPaymentAsync(
            string loanId, decimal amount, string sourceAccountId, CancellationToken ct)
        {
            var loan = await _loanRepo.FirstOrDefaultAsync(l => l.Id == loanId,null!, ct)
                      ?? throw new ItemNotFoundException("Loan not found");

            if (!loan.IsApproved) throw new InvalidOperationException("Loan not active");

            if (amount != loan.MonthlyInstallment)
                throw new InvalidOperationException("Amount must equal the scheduled installment");

            // 1) pull money from the payer’s bank account
            var payer = await _accountRepo.FirstOrDefaultAsync(a => a.Id == sourceAccountId,null!, ct)
                       ?? throw new ItemNotFoundException("Source account not found");

            if (payer.Balance < amount) throw new InvalidOperationException("Insufficient funds");

            payer.Balance -= amount;
            loan.RemainingBalance = decimal.Round(loan.RemainingBalance - amount, 2);
            loan.DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

            if (loan.RemainingBalance == 0)
                loan.AccountStatus = AccountStatus.Closed;

            await _uow.CommitAsync(ct);
            return true;
        }

       

        /*-------------------------------------------------------*/
        /* 5)  Helpers                                           */
        /*-------------------------------------------------------*/
        private async Task CommitOrThrowAsync(string action, CancellationToken ct)
        {
            if (await _uow.CommitAsync(ct) <= 0)
                throw new InvalidOperationException($"Failed while {action}.");
        }

        private async Task<string> BuildLoanEmailAsync(string name, Loan loan)
        {
            return await _email.GenerateEmailBody(
                templateName: "Loan.html",
                imageUrl: "https://bank.example.com/img/loan.png",
                header: $"Hi {name}, your loan request is in progress!",
                textBody:
                    $"""
                 • Amount: {loan.LoanAmount:N2} {loan.CurrencyType}
                 • Rate:   {loan.InterestRate:P2}
                 • Account: {loan.AccountNumber}
                 """,
                link: $"https://bank.example.com/accounts/{loan.Id}",
                linkTitle: "View my loan");
        }
    }

}
