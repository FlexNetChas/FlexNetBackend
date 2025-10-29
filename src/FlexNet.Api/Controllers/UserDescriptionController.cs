using FlexNet.Application.DTOs.UserDescription.Request;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlexNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("global-quota")]
public class UserDescriptionController : ControllerBase
{
    private readonly IUserDescriptionService _userDescriptionService;

    public UserDescriptionController(IUserDescriptionService userDescriptionService)
    {
        _userDescriptionService = userDescriptionService;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> Get(int userId)
    {
        var userDescription = await _userDescriptionService.GetUserDescriptionByUserIdAsync(userId);
        return Ok(userDescription);
    }


    /* Patch will futher on improve performance and reduce API payload
     * A user will be able to update all fields or patch individual values */
    [HttpPatch("user/{userId}")]
    public async Task<IActionResult> Patch(int userId, PatchUserDescriptionRequestDto request)
    {
        var userDescription = await _userDescriptionService.PatchUserDescriptionAsync(userId, request);
        return Ok(userDescription);
    }
}