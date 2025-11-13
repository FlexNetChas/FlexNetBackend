using FlexNet.Application.DTOs.Auth.Request;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// [Authorize] not needed on controllers — the fallback policy already requires authentication for all endpoints.
// Only endpoints marked with [AllowAnonymous] are accessible without authentication, aka Public Routes...

namespace FlexNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserContextService _userContextService;

    public AuthController(IUserService userService, ITokenService tokenService,  ILogger<AuthController> logger, IUserContextService userContextService
)
    {
        _userService = userService;
        _tokenService = tokenService;
        _userContextService = userContextService;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("public-auth")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _userService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("register")]
    [EnableRateLimiting("public-auth")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _userService.RegisterAsync(request);
        return StatusCode(201, response); 
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("public-auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var response = await _tokenService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }

    //[HttpGet("user/{id}")]
    //public async Task<IActionResult> GetUser(int id)
    //{
    //    var user = await _userService.GetByIdAsync(id);
    //    var userDto = _userService.MapToDto(user);

    //    return Ok(userDto);
    //}

    [HttpDelete("user/{id}")]
    [EnableRateLimiting("global-quota")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var requestingUserId = _userContextService.GetCurrentUserId();
        await _userService.DeleteUserAccountAsync(id, requestingUserId);
        return Ok(new { message = "Account deleted successfully" });
    }
}
