using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Attachment>> GetAllAsync();
        Task<Attachment?> GetByIdAsync(int id);
        Task<Attachment> CreateAsync(Attachment attachment);
        Task UpdateAsync(Attachment attachment);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<Attachment>> GetByComplaintIdAsync(int complaintId);
        Task<IEnumerable<Attachment>> GetNonArchivedAsync();
        Task<IEnumerable<Attachment>> GetNonArchivedByComplaintIdAsync(int complaintId);

        // Archive operations
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
        Task ArchiveBatchAsync(IEnumerable<int> ids);
    }
}
