using Microsoft.AspNetCore.Mvc;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.DTOs.ChatSession.Request;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "User")] replace with enum
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
            try
            {
                var compactChatSessions = await _chatSessionService.GetAllAsync();
                if (compactChatSessions.Any())
                    return Ok(compactChatSessions);

                return NotFound("No ChatSessions Found");
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //Get a complete ChatSession from an ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetChatSessionAsync(int id)
        {
            try
            {
                var chatSessions = await _chatSessionService.GetByIdAsync(id);
                if (chatSessions != null)
                    return Ok(chatSessions);
                
                return NotFound($"Chat session with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //Update a ChatSession, how we handle these are still under dicussion
        [HttpPatch]
        public async Task<IActionResult> UpdateChatSessionAsync([FromBody] UpdateChatSessionsRequestDto chatSession)
        {
            try
            {
                var updatedChatSession = await _chatSessionService.UpdateAsync(chatSession);
                if (updatedChatSession != null)
                    return Ok(updatedChatSession);
                return NotFound($"Chat session with ID {chatSession.Id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create a new ChatSession
        [HttpPost]
        public async Task<IActionResult> CreateChatSessionAsync([FromBody] CreateChatSessionRequestDto chatSession)
        {
            try
            {
                var createdChatSession = await _chatSessionService.CreateAsync(chatSession);
                return StatusCode(201,"Session Created");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Delete a ChatSession by ID
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteChatSessionAsync(int id)
        {
            try
            {
                var result = await _chatSessionService.DeleteAsync(id);
                if (result)
                    return Ok($"Chat session with ID {id} deleted successfully.");

                return NotFound($"Chat session with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
