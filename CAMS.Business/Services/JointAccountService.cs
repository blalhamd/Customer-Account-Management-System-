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
    public sealed class JointAccountService : IJointAccountService
    {
        private readonly IGenericRepositoryAsync<JointAccount, string> _jointRepo;
        private readonly IClientRepositoryAsync _clientRepo;
        private readonly IAccountNumberGeneratorService _generator;
        private readonly IUnitOfWorkAsync _uow;
        private readonly ILogger<JointAccountService> _log;
        private readonly IEmailBodyBuilder _email;
        private readonly IEmailSender _mailer;
        private readonly IMapper _mapper;

        public JointAccountService(
            IGenericRepositoryAsync<JointAccount, string> jointRepo,
            IClientRepositoryAsync clientRepo,
            IAccountNumberGeneratorService generator,
            IUnitOfWorkAsync uow,
            ILogger<JointAccountService> log,
            IEmailBodyBuilder email,
            IEmailSender mailer,
            IMapper mapper)
        {
            _jointRepo = jointRepo;
            _clientRepo = clientRepo;
            _generator = generator;
            _uow = uow;
            _log = log;
            _email = email;
            _mailer = mailer;
            _mapper = mapper;
        }

        /*------------------------------------------------------------
         * 1. ADD  SECONDARY HOLDER
         *-----------------------------------------------------------*/
        public async Task AddSecondaryHolderAsync(
            string jointAccountId,
            CreateSecondryHolderDto dto,
            CancellationToken ct = default)
        {
            var ja = await _jointRepo
                      .FirstOrDefaultAsync(x => x.Id == jointAccountId, null!,ct)
                     ?? throw new ItemNotFoundException("Joint account not found.");

            if (!string.IsNullOrEmpty(ja.SecondaryClientId))
                throw new ItemAlreadyExistException("Account already has a secondary holder.");

            await EnsureClientExistsAsync(dto.SecondaryClientId, ct);

            ja.SecondaryClientId = dto.SecondaryClientId;
            ja.IsEqualAccess = dto.EqualAccess;

            await _jointRepo.UpdateAsync(ja);
            await CommitOrThrowAsync("adding secondary holder", ct);
        }

        /*------------------------------------------------------------
         * 2. CREATE JOINT ACCOUNT
         *-----------------------------------------------------------*/
        public async Task<bool> CreateJointAccountAsync(
            string clientId,
            CreateJointAccountDto dto,
            CancellationToken ct = default)
        {
            var client = await _clientRepo
                         .FirstOrDefaultAsync(x => x.UserId == clientId,
                                              includes: [q => q.Include(c => c.User)],
                                              ct)
                         ?? throw new ItemNotFoundException("Primary client not found.");

            if (!string.IsNullOrEmpty(dto.SecondaryClientId))
                await EnsureClientExistsAsync(dto.SecondaryClientId, ct);

            var ja = new JointAccount
            {
                AccountNumber = await _generator.GenerateUniqueAccountNumberAsync(ct),
                CurrencyType = dto.CurrencyType,
                Branch = client.Address?.City ?? string.Empty,
                ClientId = client.Id,
                SecondaryClientId = dto.SecondaryClientId,
                IsEqualAccess = dto.IsEqualAccess,
                IsSigned = false,
                AccountStatus = AccountStatus.Pending,
                Balance = 0
            };

            await _jointRepo.AddAsync(ja, ct);
            await CommitOrThrowAsync("creating joint account", ct);

            // send mail (fire-and-forget not required; small op)
            var html = await BuildWelcomeEmailAsync(client.FullName, ja);
            await _mailer.SendEmailAsync(client.User.Email!,
                                         "Your joint account is pending activation",
                                         html);

            return true;
        }

        /*------------------------------------------------------------
         * 3. GET  (paged list + search)
         *-----------------------------------------------------------*/
        public async Task<PaginedResponse<IEnumerable<JointAccountViewModel>>>
            GetJointAccountsAsync(JointAccountQuery q, CancellationToken ct = default)
        {
            Expression<Func<JointAccount, bool>>? predicate = null;

            if (!string.IsNullOrWhiteSpace(q?.SearchBy))
            {
                var text = q.SearchBy.ToLower();
                predicate = ja =>
                       ja.AccountNumber.ToLower().Contains(text) ||
                       ja.ClientId.ToLower().Contains(text) ||
                       (ja.SecondaryClientId ?? string.Empty)
                           .ToLower()
                           .Contains(text);
            }

            var page = await _jointRepo.GetAllAsync(
                           predicate!,
                           orderBy: ja => ja.CreatedAt,
                           isAscending: false,
                           pageNumber: q.PageNumber,
                           pageSize: q.PageSize,
                           cancellationToken: ct);

            var vms = _mapper.Map<List<JointAccountViewModel>>(page.Items);

            return new PaginedResponse<IEnumerable<JointAccountViewModel>>
            {
                Items = vms,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize,
                TotalPages = page.TotalPages
            };
        }

        /*------------------------------------------------------------
         * 4. REMOVE SECONDARY HOLDER
         *-----------------------------------------------------------*/
        public async Task RemoveSecondaryHolderAsync(
            string jointAccountId,
            string secondaryClientId,
            CancellationToken ct = default)
        {
            var ja = await _jointRepo.FirstOrDefaultAsync(x => x.Id == jointAccountId, null!, ct)
                    ?? throw new ItemNotFoundException("Joint account not found.");

            if (ja.SecondaryClientId != secondaryClientId)
                throw new BadRequestException("Given client is not the current secondary holder.");

            ja.SecondaryClientId = null;
            ja.IsEqualAccess = false;

            await _jointRepo.UpdateAsync(ja);
            await CommitOrThrowAsync("removing secondary holder", ct);
        }

        /*------------------------------------------------------------
         * Helper methods
         *-----------------------------------------------------------*/
        private async Task EnsureClientExistsAsync(string clientId, CancellationToken ct)
        {
            if (!await _clientRepo.ExistsAsync(c => c.Id == clientId, ct))
                throw new ItemNotFoundException($"Client '{clientId}' not found.");
        }

        private async Task CommitOrThrowAsync(string action, CancellationToken ct)
        {
            if (await _uow.CommitAsync(ct) <= 0)
                throw new InvalidOperationException($"Failed while {action}.");
        }

        private async Task<string> BuildWelcomeEmailAsync(string fullName, JointAccount ja)
        {
            return await _email.GenerateEmailBody(
                templateName: "JointAccount.html",
                imageUrl: "https://bank.example.com/img/ja.png",
                header: $"Hi {fullName}, your joint account is pending!",
                textBody:
                    $"""
                 • Account No: {ja.AccountNumber}
                 • Equal access: {(ja.IsEqualAccess ? "Yes" : "No")}

                 You can track status anytime from online banking.
                 """,
                link: $"https://bank.example.com/accounts/{ja.Id}",
                linkTitle: "View my joint account");
        }
    }

}
