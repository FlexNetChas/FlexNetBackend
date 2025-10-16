using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices;

public interface ITokenService
{
Task<TokenPair> GenerateTokensAsync(User user);
Task<TokenPair> RefreshTokenAsync(string refreshToken);

}