using AnonymousComplaintsAPI.Models.Entities;
 
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Repositories.Implementations
{
    public class SolutionRepository : ISolutionRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public SolutionRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Solution>> GetAllAsync()
        {
            return await _context.Solutions.ToListAsync();
        }

        public async Task<Solution?> GetByIdAsync(int id)
        {
            return await _context.Solutions.FindAsync(id);
        }

        public async Task<Solution> CreateAsync(Solution solution)
        {
            _context.Solutions.Add(solution);
            await _context.SaveChangesAsync();
            return solution;
        }

        public async Task UpdateAsync(Solution solution)
        {
            _context.Entry(solution).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution != null)
            {
                _context.Solutions.Remove(solution);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Solutions.AnyAsync(e => e.SolutionId == id);
        }

        public async Task<IEnumerable<Solution>> GetByComplaintIdAsync(int complaintId)
        {
            return await _context.Solutions
                .Where(s => s.AnonymousComplaintId == complaintId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Solution>> GetNonArchivedAsync()
        {
            return await _context.Solutions
                .Where(s => s.Archived != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Solution>> GetNonArchivedByComplaintIdAsync(int complaintId)
        {
            return await _context.Solutions
                .Where(s => s.AnonymousComplaintId == complaintId && s.Archived != true)
                .ToListAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var entity = await _context.Solutions.FindAsync(id);
            if (entity != null) { entity.Archived = true; await _context.SaveChangesAsync(); }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _context.Solutions.FindAsync(id);
            if (entity != null) { entity.Archived = false; await _context.SaveChangesAsync(); }
        }
    }
}
