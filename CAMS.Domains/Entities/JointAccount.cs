namespace CAMS.Domains.Entities
{
    public class JointAccount : Account
    {
        public string? SecondaryClientId { get; set; } 
        public bool IsEqualAccess { get; set; }
    }
}

