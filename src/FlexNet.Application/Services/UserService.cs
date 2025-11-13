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
    private static readonly string DummyHash =
        "$2a$11$dummyhashtopreventtimingattack1234567890123456789012";

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

    /* We use Task.Run to offload CPU-intensive hashing to a background thread
     * This helps keep the main thread responsive, especially under load.
     * 
     * https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run
     */
    public async Task<User> CreateAsync(User user)
    {
        user.PasswordHash = await Task.Run(() =>
            BCrypt.Net.BCrypt.HashPassword(user.PasswordHash));

        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        var createdUser = await _userRepository.AddAsync(user);

        var userDescription = new UserDescription
        {
            UserId = createdUser.Id,
            Age = 0,
            Gender = null,
            Education = string.Empty,
            Purpose = string.Empty
        };

        await _userDescriptionRepo.AddUserDescriptionAsync(userDescription);
        return createdUser;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {

        var token = _jwtGenerator.GenerateAccessToken(user);
        return Task.FromResult(token);
    }

    /* To avoid timing attacks and hints. We always perform password hash verification, even if user is not found.
     * 
     * Referens:
     * https://dev.to/propelauth/understanding-timing-attacks-with-code-examples-32e6
     */
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Always verify password to avoid timing attacks 
        var passwordHash = user?.PasswordHash ?? DummyHash;
        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, passwordHash);

        if (user is null || !isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var tokens = await _tokenService.GenerateTokensAsync(user);
        var userDto = MapToDto(user);

        return new LoginResponseDto(tokens.AccessToken, tokens.RefreshToken, userDto);
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
        // Ensure user can only delete their own account
        if (userId != requestingUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own account");
        }

        var deleted = await _userRepository.DeleteAsync(userId);

        if (!deleted)
        {
            throw new KeyNotFoundException("User not found");
        }

        return deleted;
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
