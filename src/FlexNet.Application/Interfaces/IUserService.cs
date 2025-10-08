using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ValidatePasswordAsync(string email, string password);
    Task<string> GenerateJwtTokenAsync(User user);
}