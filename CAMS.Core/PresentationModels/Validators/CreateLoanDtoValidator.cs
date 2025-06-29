using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateLoanDtoValidator : AbstractValidator<CreateLoanDto>
    {
        public CreateLoanDtoValidator()
        {
            RuleFor(x => x.CurrencyType)
                  .IsInEnum()
                  .WithMessage("Currency Type is Invalid");

            RuleFor(x=> x.LoanAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Loan amount is Invalid");

        }
    }
}
