using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexNet.Infrastructure.Repositories;

/* Query optimization
 * Use AsNoTracking() for read-only queries. We don't need change tracking for READ-ONLY operations
 * Navigation props don't need to be included if we only want session metadata
 * Use OrderByDescending() when presentation data. Dosn't affect DB performance but improves UX
 * 
 * References:
 * https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying
 * https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.entityframeworkqueryableextensions.asnotracking?view=efcore-9.0
 */
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
            .AsNoTracking()
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