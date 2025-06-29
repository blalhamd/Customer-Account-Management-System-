using CAMS.Core.Constants;
using CAMS.Core.PresentationModels.DTOs.Client;
using FluentValidation;

namespace CAMS.Core.PresentationModels.Validators
{
    public class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
    {
        public CreateClientDtoValidator()
        {
            RuleFor(x => x.Email)
                          .NotEmpty().WithMessage(Errors.RequiredEmail)
                          .EmailAddress().WithMessage(Errors.InvalidEmail);

            RuleFor(x => x.Password)
                         .NotEmpty().WithMessage(Errors.RequiredPassword)
                         .Matches(RegexPatterns.PasswordPattern)
                         .WithMessage(Errors.PasswordRegExp)
                         .MinimumLength(8).WithMessage(Errors.PasswordMinLength);

            RuleFor(x => x.JobTitle)
                         .NotEmpty().WithMessage(Errors.RequiredJobTitle);

            RuleFor(x => x.Nationality)
                        .NotEmpty().WithMessage(Errors.RequiredNationality);

            RuleFor(x => x.FullName)
                        .NotEmpty().WithMessage(Errors.RequiredFullName);
            
            RuleFor(x => x.BirthDate)
                        .NotEmpty().WithMessage(Errors.RequiredBirthDate);

            RuleFor(x => x.Gender)
                         .IsInEnum().WithMessage(Errors.InvalidGender);

            RuleFor(x => x.MonthlyIncome)
                        .GreaterThanOrEqualTo(0).WithMessage(Errors.InvalidMonthlyIncome);

            RuleFor(x => x.FinancialSource)
                       .GreaterThanOrEqualTo(0).WithMessage(Errors.InvalidFinancialSource);

            RuleFor(x => x.ImagePath)
                        .NotEmpty().WithMessage(Errors.RequiredImage)
                        .Must(file =>
                              file != null &&
                              Path.HasExtension(file.FileName) &&
                              Constant.allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                        .WithMessage(Errors.InvalidImage)
                        .Must(x => x.Length <= Constant.maxFileSizeInBytes)
                        .WithMessage(Errors.InvalidImageLength);

            RuleFor(x => x.SSN)
                    .NotEmpty().WithMessage(Errors.RequiredSSN)
                    .Length(14).WithMessage(Errors.InvalidSSN)
                    .Matches(RegexPatterns.SSNPattern).WithMessage(Errors.InvalidSSNPattern);

            RuleFor(x => x.Address)
                    .NotNull().WithMessage(Errors.RequiredAddress);
        }
    }
}
