using CAMS.Domains.Entities;
using CAMS.Domains.Entities.Base;
using CAMS.Domains.Entities.Identity;
using CAMS.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;

namespace CAMS.Infrastructure.Data.context
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Current> Currents { get; set; }
        public DbSet<FixedDeposit> FixedDeposits { get; set; }
        public DbSet<JointAccount> JointAccounts { get; set; }
        public DbSet<Saving> Savings { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<BaseEntity<string>>();

            builder.Entity<Account>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Client>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Transaction>().HasQueryFilter(x => !x.IsDeleted);
           

            var cascadeFKs = builder.Model.GetEntityTypes()
                                               .SelectMany(t => t.GetForeignKeys())
                                               .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        
        public override async Task<int> SaveChangesAsync(
         CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var currentUserId = _httpContextAccessor.HttpContext?
                                .User?
                                .FindFirstValue(ClaimTypes.NameIdentifier);

            var ip = _httpContextAccessor.HttpContext?
                     .Connection
                     .RemoteIpAddress?
                     .ToString();

            // 🗒️  collect BaseEntity changes
            var tracked = ChangeTracker
                .Entries<BaseEntity<string>>()
                .Where(e => e.State is EntityState.Added
                                    or EntityState.Modified
                                    or EntityState.Deleted);

            var auditBuffer = new List<AuditEntry>(capacity: tracked.Count());

            foreach (var e in tracked)
            {
                // ---------- stamps ----------
                switch (e.State)
                {
                    case EntityState.Added:
                        e.Property(p => p.CreatedAt).CurrentValue = now;
                        e.Property(p => p.CreatedByUserId).CurrentValue = currentUserId;
                        break;

                    case EntityState.Modified:
                        if (e.Properties.Any(p => p.IsModified))
                        {
                            e.Property(p => p.UpdatedAt).CurrentValue = now;
                            e.Property(p => p.UpdatedByUserId).CurrentValue = currentUserId;
                        }
                        break;

                    case EntityState.Deleted when e.Entity is ISoftDeletable<string> soft:
                        e.State = EntityState.Modified;          // soft-delete
                        e.Entity.DoDeleted(currentUserId!);
                        break;
                }

                // ---------- audit ----------
                var audit = new AuditEntry
                {
                    ActorUserId = currentUserId ?? "SYSTEM",
                    EntityName = e.Entity.GetType().Name,
                    EntityId = e.Property("Id").CurrentValue?.ToString() ?? string.Empty,
                    Action = e.State switch
                    {
                        EntityState.Added => "CREATE",
                        EntityState.Modified => e.Entity is ISoftDeletable<string> sd && sd.IsDeleted
                                                  ? "DELETE"
                                                  : "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    },
                    Timestamp = now,
                    ChangeLogJson = BuildChangeLogJson(e),
                    IpAddress = ip,
                    Summary = string.Empty,
                    TargetUserId = currentUserId ?? "SYSTEM"
                };

                auditBuffer.Add(audit);
            }

            // enqueue all audit rows in one go
            if (auditBuffer.Count != 0)
                AuditEntries.AddRange(auditBuffer);

            return await base.SaveChangesAsync(cancellationToken);
        }

        private static string BuildChangeLogJson(EntityEntry entry)
        {
            var diffs = new Dictionary<string, object?>();

            switch (entry.State)
            {
                case EntityState.Added:
                    diffs["after"] = entry.CurrentValues.ToObject();
                    break;

                case EntityState.Deleted:
                    diffs["before"] = entry.OriginalValues.ToObject();
                    break;

                case EntityState.Modified:
                    var changes = new Dictionary<string, object?[]>(); // [old, new]
                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        changes[prop.Metadata.Name] = new[] { prop.OriginalValue, prop.CurrentValue };
                    }
                    diffs["diff"] = changes;
                    break;
            }

            return JsonSerializer.Serialize(diffs);
        }
        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var entries = ChangeTracker.Entries<BaseEntity<string>>();

        //    var CurrentUserIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        //    int? CurrentUserId = null;
        //    if (CurrentUserIdClaim != null && int.TryParse(CurrentUserIdClaim.Value, out var parsedUserId))
        //    {
        //        CurrentUserId = parsedUserId;
        //    }

        //    foreach (var entryEntity in entries)
        //    {
        //        if (entryEntity != null && CurrentUserId is not null)
        //        {
        //            if (entryEntity.State == EntityState.Added)
        //            {
        //                entryEntity.Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow;
        //                entryEntity.Property(x => x.CreatedByUserId).CurrentValue = CurrentUserId.Value.ToString();
        //            }
        //            else if (entryEntity.State == EntityState.Modified)
        //            {
        //                if (entryEntity.Properties.Any(p => p.IsModified)) //  Only update if properties are modified
        //                {
        //                    entryEntity.Property(x => x.UpdatedByUserId).CurrentValue = CurrentUserId.Value.ToString();
        //                    entryEntity.Property(x => x.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
        //                }
        //            }
        //            else if (entryEntity.State == EntityState.Deleted && entryEntity.Entity is ISoftDeletable<string>)
        //            {
        //                entryEntity.State = EntityState.Modified;
        //                entryEntity.Entity.DoDeleted(CurrentUserId.Value.ToString());
        //            }
        //        }
        //    }

        //    return await base.SaveChangesAsync(cancellationToken);
        //}
    }
}
