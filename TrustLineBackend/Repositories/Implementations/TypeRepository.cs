using AnonymousComplaintsAPI.Models.Entities;
 
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Repositories.Implementations
{
    public class TypeRepository : ITypeRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public TypeRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.Entities.Type>> GetAllAsync()
        {
            return await _context.Types.ToListAsync();
        }

        public async Task<Models.Entities.Type?> GetByIdAsync(int id)
        {
            return await _context.Types.FindAsync(id);
        }

        public async Task<Models.Entities.Type> CreateAsync(Models.Entities.Type type)
        {
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
            return type;
        }

        public async Task UpdateAsync(Models.Entities.Type type)
        {
            // Only mark specific properties as modified to avoid FK constraint issues
            // The entity is already tracked from GetByIdAsync, so we just need to save
            _context.Entry(type).Property(x => x.Name).IsModified = true;
            _context.Entry(type).Property(x => x.Archived).IsModified = true;
            // Do NOT mark CreatedBy or CreatedAt as modified - these should remain unchanged
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var type = await _context.Types.FindAsync(id);
            if (type != null)
            {
                _context.Types.Remove(type);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Types.AnyAsync(e => e.TypeId == id);
        }

        public async Task<IEnumerable<Models.Entities.Type>> GetNonArchivedAsync()
        {
            return await _context.Types
                .Where(t => t.Archived != true)
                .ToListAsync();
        }

        public async Task<Models.Entities.Type?> GetWithCategoriesAsync(int id)
        {
            return await _context.Types
                .Include(t => t.Categories)
                .FirstOrDefaultAsync(t => t.TypeId == id);
        }

        public async Task<IEnumerable<Models.Entities.Type>> GetAllWithCategoriesAsync()
        {
            return await _context.Types
                .Include(t => t.Categories)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Entities.Type>> GetNonArchivedWithCategoriesAsync()
        {
            return await _context.Types
                .Where(t => t.Archived != true)
                .Include(t => t.Categories)
                .ToListAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            await _context.Types
                .Where(t => t.TypeId == id)
                .ExecuteUpdateAsync(t => t.SetProperty(x => x.Archived, true));
        }

        public async Task RestoreAsync(int id)
        {
            await _context.Types
                .Where(t => t.TypeId == id)
                .ExecuteUpdateAsync(t => t.SetProperty(x => x.Archived, false));
        }

        public async Task<(IEnumerable<Models.Entities.Type> Data, int Total)> GetPaginatedAsync(
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take)
        {
            var query = _context.Types.Include(t => t.Categories).AsQueryable();

            // Filter by archived status
            if (includeArchived == true)
            {
                query = query.Where(t => t.Archived == true);
            }
            else
            {
                query = query.Where(t => t.Archived != true);
            }

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(lowerQuery));
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Order by CreatedAt descending
            query = query.OrderByDescending(t => t.CreatedAt);

            // Apply pagination
            var data = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (data, total);
        }
    }
}
