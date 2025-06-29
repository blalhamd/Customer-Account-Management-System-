namespace CAMS.Core.IServices
{
    public interface IUserPermissionService
    {
        Task<List<string>> GetPermissionsForUserAsync(string userId);

    }
}
