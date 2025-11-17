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
public class RefreshTokenRepo : IRefreshTokenRepo
{
    private readonly ApplicationDbContext _context;
    public RefreshTokenRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    // GenerateTokensAsync need access to User navigation prop to generate JWT
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt =>rt.User)
            .SingleOrDefaultAsync(rt =>  rt.Token == token);
        return refreshToken;

    }

    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }
}