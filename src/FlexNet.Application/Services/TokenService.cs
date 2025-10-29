
using System.Security.Cryptography;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;
using Microsoft.Extensions.Logging;
using FlexNet.Application.DTOs.Token.Response;
using FlexNet.Application.DTOs.Auth.Response; 

namespace FlexNet.Application.Services;

public class TokenService : ITokenService
{

    private readonly IRefreshTokenRepo _rtRepo;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IRefreshTokenRepo rtRepo,  IJwtGenerator jwtGenerator, ILogger<TokenService> logger)
    {
        _rtRepo = rtRepo;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
        _logger.LogInformation("TokenService initialized");

    }
    public async Task<TokenPairResponseDto> GenerateTokensAsync(User user)
    {
        var accessToken =  _jwtGenerator.GenerateAccessToken(user);

        var refreshTokenString = GenerateSecureToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            // ExpiresAt = DateTime.UtcNow.AddDays(7),
            ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            IsUsed = false,
            IsRevoked = false
        };
        
        await _rtRepo.AddAsync(refreshToken);
        _logger.LogInformation(
            "Generated new token pair for user {UserId}. Token expires at {ExpiresAt}",
            user.Id,
            refreshToken.ExpiresAt
        );
        return new TokenPairResponseDto(accessToken, refreshTokenString);
    }

    public async Task<RefreshResponseDto> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("RefreshTokenAsync called");

        var storedToken = await _rtRepo.GetByTokenAsync(refreshToken);

        if (storedToken is null )
            throw new UnauthorizedAccessException("Invalid refresh token");
        
        _logger.LogInformation(
            "Token found for user {UserId}. IsUsed: {IsUsed}, IsRevoked: {IsRevoked}, ExpiresAt: {ExpiresAt}",
            storedToken.UserId,
            storedToken.IsUsed,
            storedToken.IsRevoked,
            storedToken.ExpiresAt
        );
        
        if (storedToken.IsUsed)
        {
            _logger.LogWarning(                "TOKEN REUSE DETECTED! " +
                                               "User {UserId} attempted to reuse token that was already used at {UsedAt}. " +
                                               "Possible security breach!",
                storedToken.UserId,
                storedToken.UsedAt);
            throw new UnauthorizedAccessException("Token reuse detected. Please log in again.");

        }
        if (!IsTokenValid(storedToken))
        {
            var reason = storedToken.ExpiresAt <= DateTime.UtcNow ? "expired" : 
                storedToken.IsRevoked ? "revoked" : "invalid";
            
            _logger.LogInformation(
                "Refresh attempt with {Reason} token for user {UserId}",
                reason,
                storedToken.UserId
            );
            
            throw new UnauthorizedAccessException($"Token {reason}");
        }
        _logger.LogInformation("Marking token as used for user {UserId}", storedToken.UserId);
 
        storedToken.IsUsed = true;
        storedToken.UsedAt = DateTime.UtcNow;
        await _rtRepo.UpdateAsync(storedToken);
        _logger.LogInformation(
            "Successfully refreshed token for user {UserId}. Generating new token pair...",
            storedToken.UserId
        );
        var tokens = await GenerateTokensAsync(storedToken.User!);

        return new RefreshResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken
        );
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