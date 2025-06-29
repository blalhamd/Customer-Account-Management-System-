using CAMS.Core.PresentationModels.DTOs.Transaction;
using CAMS.Domains.Enums;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionDtoValidator()
        {
            RuleFor(x => x.TransactionType).IsInEnum().WithMessage("Transaction Type is invalid");
            RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account Number is required");
            RuleFor(x => x.Amount)
                  .GreaterThan(0)
                  .WithMessage("Amount must be greater than zero")
                  .When(x => x.TransactionType == TransactionType.Withdrawal
                  || x.TransactionType == TransactionType.Transfer
                  || x.TransactionType == TransactionType.Deposit);
        }
    }
}
