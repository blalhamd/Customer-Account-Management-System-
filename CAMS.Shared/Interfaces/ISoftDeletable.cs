namespace CAMS.Shared.Interfaces
{
    public interface ISoftDeletable<T>
    {
        T? DeleteByUserId { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}
