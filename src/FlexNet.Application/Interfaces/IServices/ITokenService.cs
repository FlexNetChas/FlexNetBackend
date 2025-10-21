using FlexNet.Application.DTOs.Token.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices;

public interface ITokenService
{
Task<TokenPairResponseDto> GenerateTokensAsync(User user);
Task<TokenPairResponseDto> RefreshTokenAsync(string refreshToken);

}