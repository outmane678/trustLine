using AnonymousComplaintsAPI.Models.Entities;
 
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Repositories.Implementations
    
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public CategoryRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            // Only mark specific properties as modified to avoid FK constraint issues
            // The entity is already tracked from GetByIdAsync, so we just need to save
            _context.Entry(category).Property(x => x.Name).IsModified = true;
            _context.Entry(category).Property(x => x.Archived).IsModified = true;
            _context.Entry(category).Property(x => x.TypeId).IsModified = true;
            _context.Entry(category).Property(x => x.ParentCategoryId).IsModified = true;
            // Do NOT mark CreatedBy or CreatedAt as modified - these should remain unchanged
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(e => e.CategoryId == id);
        }

        public async Task<IEnumerable<Category>> GetByTypeIdAsync(int typeId)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.TypeId == typeId && c.Archived != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetNonArchivedAsync()
        {
            return await _context.Categories
                .Where(c => c.Archived != true)
                .ToListAsync();
        }

        public async Task<Category?> GetWithTypeAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Type)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<IEnumerable<Category>> GetAllWithTypeAsync()
        {
            return await _context.Categories
                .Include(c => c.Type)
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            await _context.Categories
                .Where(c => c.CategoryId == id)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.Archived, true));
        }

        public async Task RestoreAsync(int id)
        {
            await _context.Categories
                .Where(c => c.CategoryId == id)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.Archived, false));
        }

        public async Task<(IEnumerable<Category> Data, int Total)> GetPaginatedAsync(
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take)
        {
            var query = _context.Categories
                .Include(c => c.Type)
                .Include(c => c.ParentCategory)
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

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerQuery));
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Order by CreatedAt descending
            query = query.OrderByDescending(c => c.CreatedAt);

            // Apply pagination
            var data = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (data, total);
        }

        public async Task<(IEnumerable<Category> Data, int Total)> GetByTypeIdPaginatedAsync(
            int typeId,
            string? searchQuery,
            bool? includeArchived,
            int skip,
            int take)
        {
            var query = _context.Categories
                .Include(c => c.Type)
                .Include(c => c.ParentCategory)
                .Where(c => c.TypeId == typeId)
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

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerQuery));
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Order by CreatedAt descending
            query = query.OrderByDescending(c => c.CreatedAt);

            // Apply pagination
            var data = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (data, total);
        }
    }
}
