using AnonymousComplaintsAPI.Models.Entities;

using Microsoft.EntityFrameworkCore.Storage;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Repositories.Implementations
{
    public class AnonymousComplaintRepository : IAnonymousComplaintRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }

        public AnonymousComplaintRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<AnonymousComplaint>> GetAllAsync()
        //{
        //    return await _context.AnonymousComplaints.ToListAsync();
        //}

        public async Task<IEnumerable<AnonymousComplaint>> GetAllAsync()
        {
            return await _context.AnonymousComplaints.ToListAsync();
        }

        public async Task<AnonymousComplaint?> GetByIdAsync(int id)
        {
            return await _context.AnonymousComplaints.FindAsync(id);
        }

        public async Task<AnonymousComplaint> CreateAsync(AnonymousComplaint complaint)
        {
            _context.AnonymousComplaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task UpdateAsync(AnonymousComplaint complaint)
        {
            _context.Entry(complaint).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var complaint = await _context.AnonymousComplaints.FindAsync(id);
            if (complaint != null)
            {
                _context.AnonymousComplaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
        }

        //public async Task<bool> ExistsAsync(int id)
        //{
        //    return await _context.AnonymousComplaints.AnyAsync(e => e.AnonymousComplaintId == id);
        //}

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.AnonymousComplaints.AnyAsync(e => e.AnonymousComplaintId == id);
        }

        //public async Task<IEnumerable<AnonymousComplaint>> GetByUserIdAsync(int userId)
        //{
        //    return await _context.AnonymousComplaints
        //        .Where(c => c.CreatedBy == userId)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<AnonymousComplaint>> GetByUserIdAsync(int userId)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousComplaint>> GetByStateAsync(string state)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.State == state)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousComplaint>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousComplaint>> GetByTypeIdAsync(int typeId)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.TypeId == typeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousComplaint>> GetNonArchivedAsync()
        {
            return await _context.AnonymousComplaints
                .Where(c => c.Archived != true)
                .ToListAsync();
        }

        public async Task<AnonymousComplaint?> GetWithDetailsAsync(int id)
        {
            return await _context.AnonymousComplaints
                .Include(c => c.Category)
                .Include(c => c.Type)
                .Include(c => c.Attachments)
                .Include(c => c.Solutions)
                .FirstOrDefaultAsync(c => c.AnonymousComplaintId == id);
        }

        //public async Task<IEnumerable<AnonymousComplaint>> GetAllWithDetailsAsync()
        //{
        //    return await _context.AnonymousComplaints
        //        .Include(c => c.Category)
        //        .Include(c => c.Type)
        //        .Include(c => c.Attachments)
        //        .Include(c => c.Solutions)
        //        .ToListAsync();
        //}

        public async Task ArchiveAsync(int id)
        {
            var entity = await _context.AnonymousComplaints.FindAsync(id);
            if (entity != null) { entity.Archived = true; await _context.SaveChangesAsync(); }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _context.AnonymousComplaints.FindAsync(id);
            if (entity != null) { entity.Archived = false; await _context.SaveChangesAsync(); }
        }

        public async Task UpdateStateAsync(int id, string newState)
        {
            var entity = await _context.AnonymousComplaints.FindAsync(id);
            if (entity != null) { entity.State = newState; await _context.SaveChangesAsync(); }
        }

        public async Task<IEnumerable<AnonymousComplaint>> GetMergedComplaintsAsync(int mainComplaintId)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.FusionWithId == mainComplaintId)
                .Include(c => c.Category)
                .Include(c => c.Type)
                .Include(c => c.Attachments.Where(a => a.Archived != true))
                .Include(c => c.Solutions.Where(s => s.Archived != true))
                .AsNoTracking() // Don't track for read-only operations
                .ToListAsync();
        }

        public async Task UpdateMultipleStatesAsync(List<int> complaintIds, string newState)
        {
            var entities = await _context.AnonymousComplaints
                .Where(c => complaintIds.Contains(c.AnonymousComplaintId))
                .ToListAsync();
            foreach (var e in entities) e.State = newState;
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<AnonymousComplaint> Data, int Total)> GetPaginatedAsync(
           bool? includeArchived,
           int? typeId = null,
           int? categoryId = null,
           string? state = null,
           DateTime? dateFrom = null,
           DateTime? dateTo = null)
        {
            var query = _context.AnonymousComplaints
                .Include(c => c.Category)
                .Include(c => c.Type)
                .AsNoTracking()
                .AsQueryable();

            // Filter by archived status
            if (includeArchived == true)
            {
                query = query.Where(c => c.Archived == true);
            }
            else
            {
                query = query.Where(c => c.Archived != true);
            }


            // Filter by Type ID
            if (typeId.HasValue)
            {
                query = query.Where(c => c.TypeId == typeId.Value);
            }

            // Filter by Category ID
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            // Filter by State
            if (!string.IsNullOrWhiteSpace(state))
            {
                query = query.Where(c => c.State == state);
            }

            // Filter by Date Range (CreatedAt)
            if (dateFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                var dateToEndOfDay = dateTo.Value.AddDays(1);
                query = query.Where(c => c.CreatedAt < dateToEndOfDay);
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Order by CreatedAt descending
            query = query.OrderByDescending(c => c.CreatedAt);

            var data = await query.ToListAsync();

            return (data, total);
        }

        public async Task<(IEnumerable<AnonymousComplaint> Data, int Total)> GetByUserAsync(
            string? searchQuery,
            bool? includeArchived,
            int? userId,
            int? typeId = null,
            int? categoryId = null,
            string? state = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.AnonymousComplaints
                .Include(c => c.Category)
                .Include(c => c.Type)
                .AsNoTracking()
                .AsQueryable();

            // Filter by archived status
            if (includeArchived == true)
            {
                query = query.Where(c => c.Archived == true);
            }
            else
            {
                query = query.Where(c => c.Archived != true);
            }

            // Filter by user ID
            if (userId.HasValue)
            {
                query = query.Where(c => c.CreatedBy == userId);
            }

            // Recherche par description 
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(c => c.Description != null && c.Description.ToLower().Contains(lowerQuery));
                
            }

            // Filter by Type ID
            if (typeId.HasValue)
            {
                query = query.Where(c => c.TypeId == typeId.Value);
            }

            // Filter by Category ID
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            // Filter by State
            if (!string.IsNullOrWhiteSpace(state))
            {
                query = query.Where(c => c.State == state);
            }

            // Filter by Date Range (CreatedAt)
            if (dateFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                // Add one day to include the entire end date
                var dateToEndOfDay = dateTo.Value.AddDays(1);
                query = query.Where(c => c.CreatedAt < dateToEndOfDay);
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Order by CreatedAt descending
            query = query.OrderByDescending(c => c.CreatedAt);

            // Apply pagination
            var data = await query
                .ToListAsync();

            return (data, total);
        }

        public async Task<AnonymousComplaint?> GetDetailsByIdAsync(int id)
        {
            return await _context.AnonymousComplaints
                .Where(c => c.AnonymousComplaintId == id)
                .Include(c => c.Category)
                .Include(c => c.Type)
                .Include(c => c.Frequency)
                .Include(c => c.Attachments.Where(a => a.Archived != true))
                .Include(c => c.Solutions.Where(s => s.Archived != true))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

    }
}

