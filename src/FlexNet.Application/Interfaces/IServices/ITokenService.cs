using FlexNet.Application.DTOs.Auth.Response;
using FlexNet.Application.DTOs.Token.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices;

public interface ITokenService
{
Task<TokenPairResponseDto> GenerateTokensAsync(User user);
Task<RefreshResponseDto> RefreshTokenAsync(string refreshToken);

}