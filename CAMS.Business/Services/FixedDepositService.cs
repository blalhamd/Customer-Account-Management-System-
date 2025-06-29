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
    public sealed class FixedDepositService : IFixedDepositService
    {
        private readonly IGenericRepositoryAsync<FixedDeposit, string> _fdRepo;
        private readonly IClientRepositoryAsync _clientRepo;
        private readonly IAccountNumberGeneratorService _generator;
        private readonly ILoanPricingService _loanPricingService;
        private readonly IUnitOfWorkAsync _uow;
        private readonly IEmailSender _mailer;
        private readonly IEmailBodyBuilder _email;
        private readonly ILogger<FixedDepositService> _log;
        private readonly IMapper _mapper;

        public FixedDepositService(
            IGenericRepositoryAsync<FixedDeposit, string> fdRepo,
            IClientRepositoryAsync clientRepo,
            IAccountNumberGeneratorService generator,
            ILoanPricingService loanPricingService,
            IUnitOfWorkAsync uow,
            IEmailSender mailer,
            IEmailBodyBuilder email,
            ILogger<FixedDepositService> log,
            IMapper mapper)
        {
            _fdRepo = fdRepo;
            _clientRepo = clientRepo;
            _generator = generator;
            _loanPricingService = loanPricingService;
            _uow = uow;
            _mailer = mailer;
            _email = email;
            _log = log;
            _mapper = mapper;
        }

        /*-----------------------------------------------------------
         * 1)  LIST / SEARCH  (paged)
         *----------------------------------------------------------*/
        public async Task<PaginedResponse<IEnumerable<FixedDepositViewModel>>>
            GetFixedDepositsAsync(FixedDepositQuery q, CancellationToken ct = default)
        {
            Expression<Func<FixedDeposit, bool>>? predicate = null;

            if (!string.IsNullOrWhiteSpace(q.SearchBy))
            {
                var text = q.SearchBy.ToLower();
                predicate = fd =>
                     fd.AccountNumber.ToLower().Contains(text) ||
                     (fd.Branch ?? string.Empty).ToLower().Contains(text) ||
                     fd.ClientId.ToLower().Contains(text);
            }

            var page = await _fdRepo.GetAllAsync(
                           predicate!,
                           orderBy: fd => fd.CreatedAt,
                           isAscending: false,
                           pageNumber: q.PageNumber,
                           pageSize: q.PageSize,
                           cancellationToken: ct);

            if (page.Items.Count() == 0)
                return new();                       // empty response object

            var vms = _mapper.Map<List<FixedDepositViewModel>>(page.Items);

            return new PaginedResponse<IEnumerable<FixedDepositViewModel>>
            {
                Items = vms,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize,
                TotalPages = page.TotalPages
            };
        }

        /*-----------------------------------------------------------
         * 2)  CRON JOB  – mature all due FDs
         *----------------------------------------------------------*/
        public async Task MatureFixedDepositAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var batch = await _fdRepo.GetAllAsync(
                            fd => !fd.IsMatured && fd.MaturityDate <= today,
                            cancellationToken: ct);

            foreach (var fd in batch.Items)
            {
                fd.InterestEarned = decimal.Round(
                    fd.DepositAmount * fd.InterestRate * fd.TermInMonths / 12m, 2);

                fd.IsMatured = true;
                fd.AccountStatus = AccountStatus.Closed;
            }

            await _fdRepo.UpdateRangeAsync(batch.Items);
            await CommitOrThrowAsync("maturing deposits", ct);
        }

        /*-----------------------------------------------------------
         * 3)  CREATE  (client request)
         *----------------------------------------------------------*/
        public async Task<bool> OpenFixedDepositAsync(
            string clientId, CreateFixedDepositDto dto, CancellationToken ct = default)
        {
            var client = await _clientRepo.FirstOrDefaultAsync(
                             c => c.UserId == clientId,
                             includes: [q => q.Include(c => c.User)],
                             ct)
                         ?? throw new ItemNotFoundException("Client not found.");

            var interestRate = await _loanPricingService.GetFixedDepositInterestRateAsync(dto.CurrencyType, dto.TermInMonths, ct);

            var fd = new FixedDeposit
            {
                ClientId = client.Id,
                AccountNumber = await _generator.GenerateUniqueAccountNumberAsync(ct),
                CurrencyType = dto.CurrencyType,
                DepositAmount = dto.DepositAmount,
                Balance = dto.DepositAmount,
                InterestRate = interestRate,
                TermInMonths = dto.TermInMonths,
                MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(dto.TermInMonths)),
                Branch = client.Address?.City ?? string.Empty,
                AccountStatus = AccountStatus.Pending,
                IsSigned = false,
                IsMatured = false,
                InterestEarned = 0
            };

            await _fdRepo.AddAsync(fd, ct);
            await CommitOrThrowAsync("opening FD", ct);

            var html = await BuildEmailBodyAsync(client.FullName, fd);
            await _mailer.SendEmailAsync(client.User.Email!,
                                         "Your fixed deposit request is pending",
                                         html);

            return true;
        }

        /*-----------------------------------------------------------
         * Helpers
         *----------------------------------------------------------*/

        private async Task<string> BuildEmailBodyAsync(string name, FixedDeposit fd)
        {
            return await _email.GenerateEmailBody(
                templateName: "FixedDeposit.html",
                imageUrl: "https://bank.example.com/img/fd.png",
                header: $"Hi {name}, your fixed deposit request is in progress!",
                textBody:
                    $"""
                 • Amount: {fd.DepositAmount:N2} {fd.CurrencyType}
                 • Term:   {fd.TermInMonths} month{(fd.TermInMonths > 1 ? "s" : "")}
                 • Rate:   {fd.InterestRate:P2}
                 • Account: {fd.AccountNumber}
                 • Maturity: {fd.MaturityDate:yyyy-MM-dd}
                 """,
                link: $"https://bank.example.com/accounts/{fd.Id}",
                linkTitle: "View my fixed deposit");
        }

        private async Task CommitOrThrowAsync(string action, CancellationToken ct)
        {
            if (await _uow.CommitAsync(ct) <= 0)
                throw new InvalidOperationException($"Failed while {action}.");
        }
        
    }

}
