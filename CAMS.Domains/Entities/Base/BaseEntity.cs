using CAMS.Shared.Interfaces;

namespace CAMS.Domains.Entities.Base
{
    public class BaseEntity<T> : IEntity<T>, IAudiotableEntity<T>, ISoftDeletable<T>
    {
        public T Id { get; set; } 
        public T? CreatedByUserId { get; set; } 
        public DateTimeOffset CreatedAt { get; set; }
        public T? UpdatedByUserId { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public T? DeleteByUserId { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

        public void DoDeleted(T userId)
        {
            DeleteByUserId = userId;
            DeletedAt = DateTimeOffset.UtcNow;
            IsDeleted = true;
        }

        public void UndoDeleted()
        {
            DeleteByUserId = default;
            DeletedAt = null;
            IsDeleted = false;
        }
    }
}
