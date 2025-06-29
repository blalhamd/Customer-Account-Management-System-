using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.User;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(Errors.RequiredEmail)
                               .EmailAddress().WithMessage(Errors.InvalidEmail);

        }
    }
}
