namespace CAMS.Core.PresentationModels.ViewModels.User
{
    public class UserViewModel
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public List<string>? Roles { get; set; } = new();
    }
}
