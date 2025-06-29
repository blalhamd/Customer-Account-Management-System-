using CAMS.Core.PresentationModels.DTOs.Account;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateSecondryHolderDtoValidator : AbstractValidator<CreateSecondryHolderDto>
    {
        public CreateSecondryHolderDtoValidator()
        {
            RuleFor(x=> x.SecondaryClientId).NotEmpty().WithMessage("Secondary Client Id is required");
        }
    }
}
