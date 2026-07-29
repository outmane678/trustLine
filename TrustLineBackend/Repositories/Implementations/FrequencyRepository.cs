using AnonymousComplaintsAPI.Models.Entities;
 
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Repositories.Implementations

{
    public class FrequencyRepository : IFrequencyRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public FrequencyRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Frequency>> GetAllAsync()
        {
            return await _context.Frequencies.ToListAsync();
        }

        public async Task<Frequency?> GetByIdAsync(int id)
        {
            return await _context.Frequencies.FindAsync(id);
        }

        public async Task<Frequency> CreateAsync(Frequency frequency)
        {
            _context.Frequencies.Add(frequency);
            await _context.SaveChangesAsync();
            return frequency;
        }

        public async Task UpdateAsync(Frequency frequency)
        {
            _context.Entry(frequency).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var frequency = await _context.Frequencies.FindAsync(id);
            if (frequency != null)
            {
                _context.Frequencies.Remove(frequency);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Frequencies.AnyAsync(e => e.FrequencyId == id);
        }

        public async Task<IEnumerable<Frequency>> GetNonArchivedAsync()
        {
            return await _context.Frequencies
                .Where(f => f.Archived != true)
                .ToListAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var entity = await _context.Frequencies.FindAsync(id);
            if (entity != null) { entity.Archived = true; await _context.SaveChangesAsync(); }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _context.Frequencies.FindAsync(id);
            if (entity != null) { entity.Archived = false; await _context.SaveChangesAsync(); }
        }
    }
}
