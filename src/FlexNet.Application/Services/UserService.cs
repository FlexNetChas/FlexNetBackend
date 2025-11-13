using FlexNet.Application.DTOs.Auth.Request;
using FlexNet.Application.DTOs.Auth.Response;
using FlexNet.Application.DTOs.User; 
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepo _userRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserDescriptionRepo _userDescriptionRepo;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepo userRepository,  IJwtGenerator jwtGenerator,
    IUserDescriptionRepo userDescriptionRepo, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _userDescriptionRepo = userDescriptionRepo; 
        _tokenService = tokenService;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
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

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {

        var token = _jwtGenerator.GenerateAccessToken(user);
        return Task.FromResult(token);
    }

    /* Return invalid credentials exception and don't specify if email or password is wrong 
     * to avoid giving hints to potential bruteforce attacks. 
     */
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var isValid = await ValidatePasswordAsync(request.Email, request.Password);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var tokens = await _tokenService.GenerateTokensAsync(user);
        var userDto = MapToDto(user);

        return new LoginResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            userDto
        );
    }

    /* Todo: We may need to swap precheck logic with DB unique constraint to avoid 
     * race conditions. Thought is unlikely in our current scale  */
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = request.Password, 
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await CreateAsync(user);
        var tokens = await _tokenService.GenerateTokensAsync(createdUser);
        var userDto = MapToDto(createdUser);

        return new RegisterResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            userDto
        );
    }

    public async Task<bool> DeleteUserAccountAsync(int userId, int requestingUserId)
    {
        // Ensure that a user can only delete their own account. requestingUserId is user id provided from the JWT token claims
        if (userId != requestingUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own account");
        }

        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException($"User not found");
        }

        try
        {
            // Let EF handle the cascade delete
            var deleted = await _userRepository.DeleteAsync(userId);

            return deleted;
        }
        catch (Exception ex)
        {
 
            throw new Exception("An error occurred while deleting the user account", ex);
        }
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


/* Removed Service - Not in used
 
    public async Task<User> UpdateAsync(User user)
    {
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

 */
