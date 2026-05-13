using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface ISolutionRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Solution>> GetAllAsync();
        Task<Solution?> GetByIdAsync(int id);
        Task<Solution> CreateAsync(Solution solution);
        Task UpdateAsync(Solution solution);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<Solution>> GetByComplaintIdAsync(int complaintId);
        Task<IEnumerable<Solution>> GetNonArchivedAsync();
        Task<IEnumerable<Solution>> GetNonArchivedByComplaintIdAsync(int complaintId);

        // Archive operations
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
    }
}
