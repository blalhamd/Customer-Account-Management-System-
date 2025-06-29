using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateSavingsDtoValidator : AbstractValidator<CreateSavingsDto>
    {
        public CreateSavingsDtoValidator()
        {
            RuleFor(x => x.DurationInMonthes).GreaterThan(0).WithMessage("duration is mondthes is not valid");
            RuleFor(x => x.Balance).GreaterThanOrEqualTo(0).WithMessage("balance is invalid");
            RuleFor(x => x.CurrencyType).IsInEnum().WithMessage("currency type is invalid");
        }
    }
}
