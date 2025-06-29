namespace CAMS.Core.PresentationModels.ViewModels.Auth
{
    public class JwtProviderResponse
    {
        public string Token { get; set; } = null!;
        public int ExpireIn { get; set; }
    }
}
