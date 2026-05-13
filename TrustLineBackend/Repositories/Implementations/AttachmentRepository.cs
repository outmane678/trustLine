using AnonymousComplaintsAPI.Models.Entities;
 
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data; 
namespace AnonymousComplaintsAPI.Repositories.Implementations
   
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AnonymousComplaintsV002Context _context;

        public AttachmentRepository(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attachment>> GetAllAsync()
        {
            return await _context.Attachments.ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(int id)
        {
            return await _context.Attachments.FindAsync(id);
        }

        public async Task<Attachment> CreateAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task UpdateAsync(Attachment attachment)
        {
            _context.Entry(attachment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Attachments.AnyAsync(e => e.AttachmentId == id);
        }

        public async Task<IEnumerable<Attachment>> GetByComplaintIdAsync(int complaintId)
        {
            return await _context.Attachments
                .Where(a => a.AnonymousComplaintId == complaintId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attachment>> GetNonArchivedAsync()
        {
            return await _context.Attachments
                .Where(a => a.Archived != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attachment>> GetNonArchivedByComplaintIdAsync(int complaintId)
        {
            return await _context.Attachments
                .Where(a => a.AnonymousComplaintId == complaintId && a.Archived != true)
                .ToListAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            await _context.Attachments
                .Where(a => a.AttachmentId == id)
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.Archived, true));
        }

        public async Task RestoreAsync(int id)
        {
            await _context.Attachments
                .Where(a => a.AttachmentId == id)
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.Archived, false));
        }

        public async Task ArchiveBatchAsync(IEnumerable<int> ids)
        {
            await _context.Attachments
                .Where(a => ids.Contains(a.AttachmentId))
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.Archived, true));
        }
    }
}
