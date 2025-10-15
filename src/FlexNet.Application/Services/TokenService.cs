using System.Security.Cryptography;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace FlexNet.Application.Services;

public class TokenService : ITokenService
{

    private readonly IRefreshTokenRepo _rtRepo;
    private readonly IJwtGenerator _jwtGenerator;

    public TokenService(IRefreshTokenRepo rtRepo,  IJwtGenerator jwtGenerator)
    {
        _rtRepo = rtRepo;
        _jwtGenerator = jwtGenerator;
    }
    public async Task<TokenPair> GenerateTokensAsync(User user)
    {
        var accessToken =  _jwtGenerator.GenerateAccessToken(user);

        var refreshTokenString = GenerateSecureToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        
        await _rtRepo.AddAsync(refreshToken);
        return new TokenPair(accessToken, refreshTokenString);
    }

    public async Task<TokenPair> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _rtRepo.GetByTokenAsync(refreshToken);
        
        if (storedToken == null || !IsTokenValid(storedToken))
            throw new InvalidOperationException("Invalid refresh token");
        
        storedToken.IsUsed = true;
        storedToken.UsedAt = DateTime.UtcNow;
        await _rtRepo.UpdateAsync(storedToken);

        return await GenerateTokensAsync(storedToken.User!);
    }

    private bool IsTokenValid(RefreshToken token)
    {
        return token.ExpiresAt > DateTime.UtcNow && !token.IsUsed && !token.IsRevoked;
    }


    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}