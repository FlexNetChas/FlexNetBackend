using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories;

public interface IRefreshTokenRepo
{
   Task<RefreshToken?> GetByTokenAsync(string token);
   Task<RefreshToken> AddAsync(RefreshToken refreshToken);
   Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);

}