namespace CAMS.Shared.Interfaces
{
    public interface IEntityModification<T>
    {
        T? UpdatedByUserId { get; set; }
        DateTimeOffset? UpdatedAt { get; set; }
    }
}
