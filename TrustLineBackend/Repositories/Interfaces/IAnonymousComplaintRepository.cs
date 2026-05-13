using AnonymousComplaintsAPI.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnonymousComplaintsAPI.Repositories.Interfaces
{
    public interface IAnonymousComplaintRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<AnonymousComplaint>> GetAllAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        IExecutionStrategy CreateExecutionStrategy();
        Task<AnonymousComplaint?> GetByIdAsync(int id);
        Task<AnonymousComplaint> CreateAsync(AnonymousComplaint complaint);
        Task UpdateAsync(AnonymousComplaint complaint);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Custom queries
        Task<IEnumerable<AnonymousComplaint>> GetByUserIdAsync(int userId);
        //Task<IEnumerable<AnonymousComplaint>> GetByStateAsync(string state);
        //Task<IEnumerable<AnonymousComplaint>> GetByCategoryIdAsync(int categoryId);
        //Task<IEnumerable<AnonymousComplaint>> GetByTypeIdAsync(int typeId);
        Task<IEnumerable<AnonymousComplaint>> GetNonArchivedAsync();

        Task<AnonymousComplaint?> GetWithDetailsAsync(int id);
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
        Task UpdateStateAsync(int id, string newState);

        // Merge
        Task<IEnumerable<AnonymousComplaint>> GetMergedComplaintsAsync(int mainComplaintId);

        Task UpdateMultipleStatesAsync(List<int> complaintIds, string newState);

        Task<(IEnumerable<AnonymousComplaint> Data, int Total)> GetPaginatedAsync(
            bool? includeArchived,
            int? typeId = null,
            int? categoryId = null,
            string? state = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);
        Task<(IEnumerable<AnonymousComplaint> Data, int Total)> GetByUserAsync(
           string? searchQuery,
           bool? includeArchived,
           int? userId,
           int? typeId = null,
           int? categoryId = null,
           string? state = null,
           DateTime? dateFrom = null,
           DateTime? dateTo = null);

        Task<AnonymousComplaint?> GetDetailsByIdAsync(int id);

    }
  
}
