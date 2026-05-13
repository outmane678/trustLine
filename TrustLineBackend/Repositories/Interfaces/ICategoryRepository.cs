using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<Category>> GetByTypeIdAsync(int typeId);
        Task<IEnumerable<Category>> GetNonArchivedAsync();
        Task<Category?> GetWithTypeAsync(int id);
        Task<IEnumerable<Category>> GetAllWithTypeAsync();

        // Archive operations
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);

        // Pagination
        Task<(IEnumerable<Category> Data, int Total)> GetPaginatedAsync(
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take);

        Task<(IEnumerable<Category> Data, int Total)> GetByTypeIdPaginatedAsync(
            int typeId,
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take);
    }
}
