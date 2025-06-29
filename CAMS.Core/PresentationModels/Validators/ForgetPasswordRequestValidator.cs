using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Auth;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(Errors.RequiredEmail)
                                  .EmailAddress().WithMessage(Errors.InvalidEmail);

        }
    }
}
