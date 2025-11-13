using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories; 

public interface IUserRepo
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    //Task<IEnumerable<User>> GetAllAsync();
    Task<User> AddAsync(User user);
    //Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    //Task<bool> ExistsAsync(int id);
}