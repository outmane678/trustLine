using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AnonymousComplaintsAPI.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AnonymousComplaintsV002Context _context;

    public UserRepository(AnonymousComplaintsV002Context context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Users.AsQueryable();

        //if (!includeArchived)
        //    query = query.Where(u => !u.Archived);

        return await query.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Entry(user).Property(x => x.Archive).IsModified = true;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task ArchiveAsync(int userId)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Archive, true));
    }

    public async Task RestoreAsync(int userId)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Archive, false));
    }
}
