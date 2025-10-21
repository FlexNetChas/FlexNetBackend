using FlexNet.Application.DTOs.UserDescription.Request;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FlexNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        try
        {
            var userDescription = await _userDescriptionService.GetUserDescriptionByUserIdAsync(userId);
            if (userDescription == null)
            {
                return NotFound(new { message = "User description not found" });
            }

            return Ok(userDescription);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }


    // Patch will futher on improve performance and reduce API payload
    // A user will be able to update all fields or patch individual values 
    [HttpPatch("user/{userId}")]
    public async Task<IActionResult> Patch(int userId, PatchUserDescriptionRequestDto request)
    {
        try
        {
            var userDescription = await _userDescriptionService.PatchUserDescriptionAsync(userId, request);

            if (userDescription == null)
                return NotFound(new { message = "User description not found" });

            return Ok(userDescription);
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}