namespace CAMS.Core.PresentationModels.ViewModels.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public int ExpireIn { get; set; }
    }
}
