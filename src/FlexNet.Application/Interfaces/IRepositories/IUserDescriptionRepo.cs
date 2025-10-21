using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories;

public interface IUserDescriptionRepo
{
    Task<UserDescription?> GetUserDescriptionByUserIdAsync(int userId);
    Task<UserDescription> AddUserDescriptionAsync(UserDescription userDescription);
    Task<UserDescription> UpdateUserDescriptionAsync(UserDescription userDescription);
}