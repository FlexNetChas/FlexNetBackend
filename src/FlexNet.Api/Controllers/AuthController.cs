﻿using FlexNet.Application.DTOs.Auth.Request;
using FlexNet.Application.DTOs.Auth.Response;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("public-auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _userService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("register")]
    [EnableRateLimiting("public-auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = response.User.Id }, response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("public-auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var response = await _tokenService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }

    [HttpGet("user/{id}")]
    [Authorize]
    [EnableRateLimiting("global-quota")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        var userDto = _userService.MapToDto(user);

        return Ok(userDto);
    }
}
