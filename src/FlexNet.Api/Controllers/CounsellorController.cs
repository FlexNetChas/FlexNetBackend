using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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