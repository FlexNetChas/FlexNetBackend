using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories; 

public interface IUserRepo
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<bool> DeleteAsync(int id);
}