using CAMS.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace CAMS.Core.PresentationModels.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = Errors.PasswordMinLength)]
        [Display(Name = "New Password")]
        [RegularExpression(RegexPatterns.PasswordPattern, ErrorMessage = Errors.PasswordRegExp)]
        public string NewPassword { get; set; } = string.Empty;
    }

}
