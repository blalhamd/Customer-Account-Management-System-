using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateJointAccountDtoValidator : AbstractValidator<CreateJointAccountDto>
    {
        public CreateJointAccountDtoValidator()
        {
            RuleFor(x => x.CurrencyType).IsInEnum().WithMessage("Currency Type is Invalid");
        }
    }
}
