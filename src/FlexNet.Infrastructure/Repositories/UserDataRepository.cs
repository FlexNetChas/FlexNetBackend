using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexNet.Infrastructure.Repositories;

public class UserDataRepository : IUserDataRepo
{
    private readonly ApplicationDbContext _context;

    public UserDataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetCompleteUserDataAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserDescription)
            .Include(u => u.ChatSessions)
            .ThenInclude(cs => cs.ChatMessages)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}