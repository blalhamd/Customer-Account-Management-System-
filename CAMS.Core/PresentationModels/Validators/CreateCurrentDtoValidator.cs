using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateCurrentDtoValidator : AbstractValidator<CreateCurrentDto>
    {
        public CreateCurrentDtoValidator()
        {
            RuleFor(x=> x.Balance).GreaterThanOrEqualTo(100).WithMessage("Balance must greater than or equal to 100.");
            RuleFor(x => x.CurrencyType).IsInEnum().WithMessage("Currency type is invalid");
        }
    }
}
