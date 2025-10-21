using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.Models; // Add this for Task
using FlexNet.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;
using System.Threading.Tasks;

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }
            try
            {
                var response = await _sendMessage.ExecuteAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}