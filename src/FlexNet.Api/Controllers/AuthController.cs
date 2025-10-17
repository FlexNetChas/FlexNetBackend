using FlexNet.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;
using FlexNet.Application.DTOs.Auth.Request;
using FlexNet.Application.DTOs.Auth.Response;

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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
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
            var userDto = _userService.MapToDto(user);
            var response = new LoginResponseDto(
                tokens.AccessToken,
                tokens.RefreshToken,
                userDto
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
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
            var userDto = _userService.MapToDto(createdUser);
            var response = new RegisterResponseDto(
                tokens.AccessToken,
                tokens.RefreshToken,
                userDto
            );

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, response);

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        try
        {
            var tokens = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            var response = new RefreshResponseDto(
                tokens.AccessToken,
                tokens.RefreshToken
            );

            return Ok(response);
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

            var userDto = _userService.MapToDto(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
