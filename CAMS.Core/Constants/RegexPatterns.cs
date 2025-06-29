namespace CAMS.Core.Constants
{
    public class RegexPatterns
    {
        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";
        public const string SSNPattern = @"^\d{14}$";
    }
}


