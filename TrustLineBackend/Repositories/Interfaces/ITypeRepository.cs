using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface ITypeRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Models.Entities.Type>> GetAllAsync();
        Task<Models.Entities.Type?> GetByIdAsync(int id);
        Task<Models.Entities.Type> CreateAsync(Models.Entities.Type type);
        Task UpdateAsync(Models.Entities.Type type);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<Models.Entities.Type>> GetNonArchivedAsync();
        Task<Models.Entities.Type?> GetWithCategoriesAsync(int id);
        Task<IEnumerable<Models.Entities.Type>> GetAllWithCategoriesAsync();
        Task<IEnumerable<Models.Entities.Type>> GetNonArchivedWithCategoriesAsync();

        // Archive operations
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);

        // Pagination
        Task<(IEnumerable<Models.Entities.Type> Data, int Total)> GetPaginatedAsync(
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take);
    }
}
