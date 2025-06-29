using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.User;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(Errors.RequiredEmail)
                                .EmailAddress().WithMessage(Errors.InvalidEmail);

            RuleFor(x => x.Password)
                         .NotEmpty().WithMessage(Errors.RequiredPassword)
                         .Matches(RegexPatterns.PasswordPattern)
                         .WithMessage(Errors.PasswordRegExp)
                         .MinimumLength(8).WithMessage(Errors.PasswordMinLength);
        }
    }
}
