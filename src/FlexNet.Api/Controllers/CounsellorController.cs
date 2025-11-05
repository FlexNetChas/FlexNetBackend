using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// [Authorize] not needed on controllers — the fallback policy already requires authentication for all endpoints.
// Only endpoints marked with [AllowAnonymous] are accessible without authentication, aka Public Routes...

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("authenticated-counsellor")]
    public class CounsellorController : ControllerBase
    {
        private readonly SendCounsellingMessage _sendMessage;

        public CounsellorController(SendCounsellingMessage sendMessage)
        {
            _sendMessage = sendMessage;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
        {
            var response = await _sendMessage.ExecuteAsync(request);
            return Ok(response);
        }
        
    }
}