using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexNet.Infrastructure.Repositories;

public class UserDescriptionRepository : IUserDescriptionRepo
{
    private readonly ApplicationDbContext _context;

    public UserDescriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDescription?> GetUserDescriptionByUserIdAsync(int userId)
    {
        return await _context.UserDescriptions
            .FirstOrDefaultAsync(userDescription => userDescription.UserId == userId);
    }

    public async Task<UserDescription> AddUserDescriptionAsync(UserDescription userDescription)
    {
        _context.UserDescriptions.Add(userDescription);
        await _context.SaveChangesAsync();
        return userDescription;
    }

    public async Task<UserDescription> UpdateUserDescriptionAsync(UserDescription userDescription)
    {
        _context.UserDescriptions.Update(userDescription);
        await _context.SaveChangesAsync();
        return userDescription;
    }
}