namespace CAMS.Core.Constants
{
    public class JwtSetting
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int LifeTime { get; set; }
        public string Key { get; set; } = null!;
    }
}


