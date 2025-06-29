using CAMS.Shared.Interfaces;

namespace CAMS.Domains.Entities
{
    public class PasswordResetCode : IEntity<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string IPAddress { get; set; } = string.Empty;
    }
}
