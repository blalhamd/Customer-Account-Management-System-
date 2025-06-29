using System.ComponentModel.DataAnnotations;

namespace CAMS.Core.PresentationModels.DTOs.Auth
{
    public class ForgetPasswordRequest 
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

}
