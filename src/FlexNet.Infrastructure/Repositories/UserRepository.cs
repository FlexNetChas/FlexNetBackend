using Microsoft.EntityFrameworkCore;
using FlexNet.Domain.Entities;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Infrastructure.Data;

namespace FlexNet.Infrastructure.Repositories;

public class UserRepository : IUserRepo
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // To avoid overfetching data have we commented out the Includes for now. 
    // This will improve queries performance 
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .AsSplitQuery()
            //.Include(u => u.Avatar)
            //.Include(u => u.UserDescription)
            //.Include(u => u.ChatSessions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deletedCount = await _context.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync();

        return deletedCount > 0;
    }
}


/*  Removed for now to avoid overfetching data and unsued calls
 
     public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Avatar)
            .Include(u => u.UserDescription)
            .Include(u => u.ChatSessions)
            .ToListAsync();
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

 */