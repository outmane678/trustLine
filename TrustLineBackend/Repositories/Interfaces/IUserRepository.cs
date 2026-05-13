using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces;

/// <summary>
/// Repository for User entity CRUD operations
/// </summary>
public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(bool includeArchived = false);
    Task<User?> GetByIdAsync(int userId);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(int userId);
    Task ArchiveAsync(int userId);
    Task RestoreAsync(int userId);
}
