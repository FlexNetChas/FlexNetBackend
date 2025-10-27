using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;
using FlexNet.Application.DTOs.User; 

namespace FlexNet.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepo _userRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserDescriptionRepo _userDescriptionRepo; 

    public UserService(IUserRepo userRepository,  IJwtGenerator jwtGenerator,
    IUserDescriptionRepo userDescriptionRepo)
    {
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _userDescriptionRepo = userDescriptionRepo; 
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        // Hash password before saving
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        // Create user
        var createdUser = await _userRepository.AddAsync(user);

        // Create default user description to to improve performance and less API requests
        // A user will only be able to update (initial login) or patch descriptions later
        var userDescription = new UserDescription
        {
            UserId = createdUser.Id,
            Age = 0,
            Gender = null,
            Education = string.Empty,
            Purpose = string.Empty
        };

        // Save UserDescription
        await _userDescriptionRepo.AddUserDescriptionAsync(userDescription);
        return createdUser;
    }

    public async Task<User> UpdateAsync(User user)
    {
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {

        var token = _jwtGenerator.GenerateAccessToken(user);
        return Task.FromResult(token);
    }

    public UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        );
    }
}