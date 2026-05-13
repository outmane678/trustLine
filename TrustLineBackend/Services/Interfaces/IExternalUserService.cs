using AnonymousComplaintsAPI.DTOs.External;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for external user CRUD operations
/// </summary>
public interface IExternalUserService
{
    /// <summary>
    /// Get all users, optionally including archived
    /// </summary>
    Task<IEnumerable<ExternalUserResponse>> GetAllUsersAsync(bool includeArchived = false);

    /// <summary>
    /// Get a single user by ID
    /// </summary>
    Task<ExternalUserResponse?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Create a new user
    /// </summary>
    Task<ExternalUserResponse> CreateUserAsync(CreateExternalUserRequest request);

    /// <summary>
    /// Update a user
    /// </summary>
    Task<ExternalUserResponse?> UpdateUserAsync(int userId, UpdateExternalUserRequest request);

    /// <summary>
    /// Check if a user exists
    /// </summary>
    Task<bool> UserExistsAsync(int userId);

    /// <summary>
    /// Archive a user (soft delete)
    /// </summary>
    Task ArchiveUserAsync(int userId);

    /// <summary>
    /// Restore an archived user
    /// </summary>
    Task RestoreUserAsync(int userId);
}
