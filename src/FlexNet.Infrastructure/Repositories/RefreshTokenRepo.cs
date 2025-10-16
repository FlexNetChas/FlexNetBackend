using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexNet.Infrastructure.Repositories;

public class RefreshTokenRepo : IRefreshTokenRepo
{
    private readonly ApplicationDbContext _context;
    public RefreshTokenRepo(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.Include(rt =>rt.User).SingleOrDefaultAsync(rt =>  rt.Token == token);
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