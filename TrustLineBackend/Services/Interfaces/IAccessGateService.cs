namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for interacting with AccessGate external API for permission checking
/// </summary>
public interface IAccessGateService
{
    /// <summary>
    /// Check if a user has a specific permission via AccessGate
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <param name="permission">The permission name (e.g. "tl-v-report")</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    Task<bool> CheckPermissionAsync(int userId, string permission);
}
