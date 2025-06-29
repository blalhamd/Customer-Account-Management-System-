namespace CAMS.Core.IServices
{
    public interface IAccountNumberGeneratorService
    {
        Task<string> GenerateUniqueAccountNumberAsync(CancellationToken ct = default);
    }
}
