using CAMS.Shared.Interfaces;

namespace CAMS.Domains.Entities
{
    public class AuditEntry : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string ActorUserId { get; init; } = null!;  // اللي نفّذ الإجراء
        public string TargetUserId { get; init; } = null!; // المستخدم الذي تأثّر
        public string Action { get; init; } = null!; // "UpdateEmail", "ResetPwd", ...
        public string? EntityName { get; init; } = null!; // "Account", "Transaction"
        public string? EntityId { get; init; } = null!;
        public DateTimeOffset Timestamp { get; init; } 
        public string Summary { get; init; } = null!;  // Before / After snapshot أو diff
        public string? IpAddress { get; init; }
        public string ChangeLogJson { get; init; } = null!;
    }
}

