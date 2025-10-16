using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories;

public interface IRefreshTokenRepo
{
   Task<RefreshToken?> GetByTokenAsync(string token);
   // Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
   Task<RefreshToken> AddAsync(RefreshToken refreshToken);
   Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
   // Task<bool> DeleteAsync(int id);
   // Task<int> DeleteExpiredTokensAsync();
   // Task<bool> ExistsAsync(string token);
}