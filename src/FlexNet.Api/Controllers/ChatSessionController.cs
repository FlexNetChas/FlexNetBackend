using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("global-quota")]
    public class ChatSessionController : ControllerBase
    {
        private readonly IChatSessionService _chatSessionService;

        public ChatSessionController(IChatSessionService service)
        {
            _chatSessionService = service;
        }

        //Get compact versions of all ChatSessions
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var compactChatSessions = await _chatSessionService.GetAllAsync();
            return Ok(compactChatSessions);
        }

        //Get a complete ChatSession from an ID
        [HttpGet("{sessionID:int}")]
        public async Task<IActionResult> GetChatSessionAsync(int sessionID)
        {
            var chatSession = await _chatSessionService.GetByIdAsync(sessionID);
            return Ok(chatSession);
        }

        //Update a ChatSession, how we handle these are still under dicussion
        [HttpPatch]
        public async Task<IActionResult> UpdateChatSessionAsync([FromBody] UpdateChatSessionsRequestDto chatSession)
        {
            var updatedChatSession = await _chatSessionService.UpdateAsync(chatSession);
            return Ok(updatedChatSession);
        }

        // Create a new ChatSession
        [HttpPost]
        public async Task<IActionResult> CreateChatSessionAsync([FromBody] CreateChatSessionRequestDto chatSession)
        {
            var createdChatSession = await _chatSessionService.CreateAsync(chatSession);
            return StatusCode(201, createdChatSession);
        }
        // End a Session
        [HttpPost("{sessionId}/end")]
        public async Task<IActionResult> EndSessionAsync(int sessionId)
        {
            var result = await _chatSessionService.EndSessionAsync(sessionId);
            return Ok(result);
        }
        // Delete a ChatSession by ID
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteChatSessionAsync(int id)
        {
            await _chatSessionService.DeleteAsync(id);
            return NoContent();
        }
    }
}