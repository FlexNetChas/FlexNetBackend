using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// [Authorize] not needed on controllers — the fallback policy already requires authentication for all endpoints.
// Only endpoints marked with [AllowAnonymous] are accessible without authentication, aka Public Routes...

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/message")]
    [EnableRateLimiting("authenticated-counsellor")]
    public class CounsellorController : ControllerBase
    {
        private readonly SendCounsellingMessage _sendMessage;
        private readonly SendCounsellingMessageStreaming _sendMessageStreaming;

        public CounsellorController(SendCounsellingMessage sendMessage, SendCounsellingMessageStreaming sendMessageStreaming)
        {
            _sendMessage = sendMessage;
            _sendMessageStreaming = sendMessageStreaming;
        }

        [HttpPost("")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
        {
            var response = await _sendMessage.ExecuteAsync(request);
            return Ok(response);
        }
        [HttpGet("stream")]
        public async Task StreamMessage([FromQuery] string message, [FromQuery] int? chatSessionId = null)
        {
            //1. Set SSE headers
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var request = new SendMessageRequestDto(message, chatSessionId);

            try
            {
                // 2. Call your use case (need to create streaming version)
                await foreach (var result in _sendMessageStreaming.ExecuteAsync(request))
                {
                    if (result.IsSuccess)
                    {
                        // 3. Write SSE data event
                        await Response.WriteAsync($"event: data\n");
                        await Response.WriteAsync($"data: {result.Data}\n\n");
                        await Response.Body.FlushAsync();
                    }
                    else
                    {
                        // 4. Write SSE error event
                        await Response.WriteAsync($"event: error\n");
                        await Response.WriteAsync($"data: {{\"error\":\"{result.Error?.Message}\"}}\n\n");
                        await Response.Body.FlushAsync();
                        break;
                    }
                }
                // 5. Write done event
                await Response.WriteAsync($"event: done\n");
                await Response.WriteAsync($"data: {{}}\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                // 6. Handle unexpected errors
                await Response.WriteAsync($"event: error\n");
                await Response.WriteAsync($"data: {{\"error\":\"{ex.Message}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}