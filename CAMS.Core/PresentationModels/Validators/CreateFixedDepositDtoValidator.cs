using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateFixedDepositDtoValidator : AbstractValidator<CreateFixedDepositDto>
    {
        public CreateFixedDepositDtoValidator()
        {
            RuleFor(x => x.CurrencyType).IsInEnum().WithMessage("Invalid selected currency type");
            RuleFor(x => x.DepositAmount).GreaterThanOrEqualTo(0).WithMessage("Invalid Deposit Amount");
        }
    }
}
