using AutoMapper;
using CAMS.Core.Constants;
using CAMS.Core.IRepositories.Generic;
using CAMS.Core.IServices;
using CAMS.Core.IServices.Email;
using CAMS.Core.IUnit;
using CAMS.Core.PresentationModels.DTOs.Transaction;
using CAMS.Core.PresentationModels.ViewModels.Transaction;
using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using CAMS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CAMS.Business.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IGenericRepositoryAsync<Transaction, string> _transactionRepo;
        private readonly IGenericRepositoryAsync<Account, string> _accountRepo;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public TransactionService(IGenericRepositoryAsync<Transaction, string> transactionRepo, IGenericRepositoryAsync<Account, string> accountRepo, IUnitOfWorkAsync unitOfWork, ILogger<AccountService> logger, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _mapper = mapper;
        }


        public async Task<TransactionViewModel> CreateTransactionAsync(string userId,CreateTransactionDto dto, CancellationToken ct = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
                    // Retrieve the source account
                    var account = await _accountRepo.FirstOrDefaultAsync(
                        x => x.AccountNumber.Trim() == dto.AccountNumber.Trim(),
                        [q => q.Include(x => x.Client).ThenInclude(x => x.User)],
                        ct
                    ) ?? throw new ItemNotFoundException("Account not found");

                    // Validate transaction type and account status
                    if (!account.IsSigned || account.AccountStatus != AccountStatus.Active)
                        throw new BadRequestException("Account is inactive or not signed");

                    var transaction = new Transaction
                    {
                        Amount = dto.Amount,
                        AccountId = account.Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        TransactionType = dto.TransactionType,
                        CreatedByUserId = userId,
                    };

                   
                    // Handle different transaction types
                    switch (dto.TransactionType)
                    {
                        case TransactionType.Withdrawal:
                            if (account.Balance < dto.Amount) throw new BadRequestException("Insufficient funds");
                            transaction.Discount = CalculateTransactionFee(dto.Amount);
                            account.Balance -= (dto.Amount + transaction.Discount);
                            break;

                        case TransactionType.Deposit:
                            if (dto.Amount <= 0) throw new BadRequestException("Invalid deposit amount");
                            transaction.Discount = 0;
                            account.Balance += dto.Amount;
                            break;

                        case TransactionType.Query:
                            transaction.Discount = 0;
                            transaction.Amount = account.Balance;
                            break;

                        case TransactionType.Transfer:
                            if (dto.Amount <= 0 || string.IsNullOrEmpty(dto.To))
                                throw new BadRequestException("Invalid transfer transaction");

                            var secondAccount = await _accountRepo.FirstOrDefaultAsync(
                                x => x.AccountNumber == dto.To,
                                [q => q.Include(x => x.Client).ThenInclude(x => x.User)],
                                ct
                            ) ?? throw new ItemNotFoundException("Second account not found");

                            transaction.Discount = CalculateTransactionFee(dto.Amount);
                            account.Balance -= (dto.Amount + transaction.Discount);
                            secondAccount.Balance += dto.Amount;

                            // Update second account
                            await _accountRepo.UpdateAsync(secondAccount);
                            break;

                        default:
                            throw new BadRequestException("Invalid transaction type");
                    }

                    if (account is Current cu)
                    {
                        if (cu.TransactionLimit == 0)
                            throw new BadRequestException("Can't make this tranaction because you reach to end Transaction limit");
                        cu.TransactionLimit--;
                    }

                    if (account is Saving sa)
                    {
                        if (sa.WithdrawalLimit == 0)
                            throw new BadRequestException("Can't make this tranaction because you reach to end Transaction limit");
                        sa.WithdrawalLimit--;
                    }

                    // Save transaction and commit changes
                    await _transactionRepo.AddAsync(transaction, ct);
                    await _accountRepo.UpdateAsync(account);
                    var rowsAffected = await _unitOfWork.CommitAsync(ct);

                    if (rowsAffected <= 0)
                        throw new InvalidOperationException("Failed to process transaction");

                    // Send email notifications
                    var body = await BuildEmailBodyAsync(account.Client.FullName, transaction.TransactionType.ToString(), account, dto.To);
                    await _emailSender.SendEmailAsync(account.Client.User.Email!, "Transaction Notification", body);


                    // Return view model
                    return _mapper.Map<TransactionViewModel>(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing transaction");
                    throw;
                }
            });
        
        }


        public async Task<TransactionViewModel?> GetTransactionByIdAsync(string transactionId, CancellationToken ct = default)
        {
            var transaction = await _transactionRepo.FirstOrDefaultAsync(x => x.Id == transactionId, null!, ct);

            if(transaction is null)
            {
                _logger.LogWarning($"transaction not found with ID {transactionId}");
                throw new ItemNotFoundException("transaction not found");
            }

            var transactionVM = _mapper.Map<TransactionViewModel>(transaction);

            return transactionVM;
        }

        public async Task<PaginedResponse<IEnumerable<TransactionViewModel>>> GetTransactionsAsync(TransactionQuery q, CancellationToken ct = default)
        {
            Expression<Func<Transaction, bool>>? predicate = null;

            if (!string.IsNullOrWhiteSpace(q.SearchBy))
            {
                var text = q.SearchBy.ToLower();
                predicate = t =>
                     t.AccountId.ToLower().Contains(text) ||
                     t.Id.ToLower().Contains(text);
            }

            var page = await _transactionRepo.GetAllAsync(
                           predicate!,
                           orderBy: fd => fd.CreatedAt,
                           isAscending: false,
                           pageNumber: q.PageNumber,
                           pageSize: q.PageSize,
                           cancellationToken: ct);

            if (page.Items.Count() == 0)
                return new();           // empty response object

            var vms = _mapper.Map<List<TransactionViewModel>>(page.Items);

            return new PaginedResponse<IEnumerable<TransactionViewModel>>
            {
                Items = vms,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize,
                TotalPages = page.TotalPages
            };
        }

        private decimal CalculateTransactionFee(decimal amount)
        {
            // For example: Fee is 1% of transaction amount
            return amount / 1000 * 10;
        }

        private async Task<string> BuildEmailBodyAsync(string name, string type, Account account, string to)
        {
            return await _emailBodyBuilder.GenerateEmailBody(
                templateName: "Transaction.html",
                imageUrl: "https://bank.example.com/img/transaction.png",
                header: $"Hi {name}, your {type} transaction was successful!",
                textBody: $"""
                • Account: {account.AccountNumber}
                • Transaction Type: {type}
                • Amount: {account.Balance:N2} {account.CurrencyType}
                • To Account: {to}
                • Date: {account.CreatedAt:yyyy-MM-dd}
            """,
                link: $"https://bank.example.com/accounts/{account.Id}",
                linkTitle: "View transaction details"
            );
        }

    }
}
