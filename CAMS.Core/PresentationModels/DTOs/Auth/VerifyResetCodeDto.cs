using System.ComponentModel.DataAnnotations;

namespace CAMS.Core.PresentationModels.DTOs.Auth
{
    public class VerifyResetCodeDto 
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

}
