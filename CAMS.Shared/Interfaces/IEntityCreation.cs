namespace CAMS.Shared.Interfaces
{
    public interface IEntityCreation<T>
    {
        T? CreatedByUserId { get; set; }
        DateTimeOffset CreatedAt { get; set; } 
    }
}
