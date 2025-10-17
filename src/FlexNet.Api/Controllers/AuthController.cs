using FlexNet.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FlexNet.Domain.Entities;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Identity.Data;
namespace FlexNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IUserService userService, ITokenService tokenService,  ILogger<AuthController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var isValid = await _userService.ValidatePasswordAsync(request.Email, request.Password);
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var tokens = await _tokenService.GenerateTokensAsync(user);

            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                user = new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = request.Password, // Will be hashed in service
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userService.CreateAsync(user);
            
            var tokens = await _tokenService.GenerateTokensAsync(createdUser);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                id = createdUser.Id,
                firstName = createdUser.FirstName,
                lastName = createdUser.LastName,
                email = createdUser.Email,
                role = createdUser.Role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var tokens = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken
            });
        }
        catch (SecurityException ex)
        {
            _logger.LogError(ex, "Security exception during token refresh");

            return Unauthorized(new { message = "Security viaolation detected", error = "token_reuse" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during refresh");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during refresh");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt,
                isActive = user.IsActive
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}