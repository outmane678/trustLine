using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface IFrequencyRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Frequency>> GetAllAsync();
        Task<Frequency?> GetByIdAsync(int id);
        Task<Frequency> CreateAsync(Frequency frequency);
        Task UpdateAsync(Frequency frequency);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<Frequency>> GetNonArchivedAsync();

        // Archive operations
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
    }
}
