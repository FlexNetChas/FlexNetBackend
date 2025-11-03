using FlexNet.Application.DTOs.Auth.Request;
using FlexNet.Application.DTOs.Auth.Response;
using FlexNet.Application.DTOs.User;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices; 

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
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);


    // Mapping method in userservice to separate mapping logic from API layer
    UserDto MapToDto(User user);
}