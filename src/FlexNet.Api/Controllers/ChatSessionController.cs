using Microsoft.AspNetCore.Mvc;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.DTOs.ChatSession.Request;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using FlexNet.Infrastructure.Migrations;

namespace FlexNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                int userID = ExtractUserID();
                switch (userID)
                {
                    case (int)ExtractUserIDResult.MISSING_TOKEN:
                        return Unauthorized("Token is missing.");
                    case (int)ExtractUserIDResult.MISSING_USER_ID:
                        return Unauthorized("User ID is missing in the token.");
                    default:
                        break;
                }

                var compactChatSessions = await _chatSessionService.GetAllAsync(userID);
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
        [HttpGet("{sessionID:int}")]
        public async Task<IActionResult> GetChatSessionAsync(int sessionID)
        {
            try
            {
                int userID = ExtractUserID();
                switch (userID)
                {
                    case (int)ExtractUserIDResult.MISSING_TOKEN:
                        return Unauthorized("Token is missing.");
                    case (int)ExtractUserIDResult.MISSING_USER_ID:
                        return Unauthorized("User ID is missing in the token.");
                    default:
                        break;
                }

                var chatSessions = await _chatSessionService.GetByIdAsync(sessionID, userID);
                if (chatSessions != null)
                    return Ok(chatSessions);
                
                return NotFound($"Chat session with ID {sessionID} not found.");
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
                int userID = ExtractUserID();
                switch (userID)
                {
                    case (int)ExtractUserIDResult.MISSING_TOKEN:
                        return Unauthorized("Token is missing.");
                    case (int)ExtractUserIDResult.MISSING_USER_ID:
                        return Unauthorized("User ID is missing in the token.");
                    default:
                        break;
                }

                var updatedChatSession = await _chatSessionService.UpdateAsync(chatSession, userID);
                if (updatedChatSession != null)
                    return Ok(updatedChatSession);
                return NotFound($"Chat session with ID {chatSession.SessionID} not found.");
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
                int userID = ExtractUserID();
                switch (userID)
                {
                    case (int)ExtractUserIDResult.MISSING_TOKEN:
                        return Unauthorized("Token is missing.");
                    case (int)ExtractUserIDResult.MISSING_USER_ID:
                        return Unauthorized("User ID is missing in the token.");
                    default:
                        break;
                }

                var createdChatSession = await _chatSessionService.CreateAsync(chatSession, userID);
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
                int userID = ExtractUserID();
                switch (userID)
                {
                    case (int)ExtractUserIDResult.MISSING_TOKEN:
                        return Unauthorized("Token is missing.");
                    case (int)ExtractUserIDResult.MISSING_USER_ID:
                        return Unauthorized("User ID is missing in the token.");
                    default:
                        break;
                }

                var result = await _chatSessionService.DeleteAsync(id, userID);
                if (result)
                    return Ok($"Chat session with ID {id} deleted successfully.");

                return NotFound($"Chat session with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private enum ExtractUserIDResult
        {
            MISSING_TOKEN = -1,
            MISSING_USER_ID = -2
        }

        private int ExtractUserID()
        {
            // Extract the JWT token from the session cookie
            var token = Request.Cookies["session"];

            if (string.IsNullOrEmpty(token))
                return (int)ExtractUserIDResult.MISSING_TOKEN;

            // Decode the JWT token to extract the userID
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub");
            
            if (userIdClaim == null)
                return (int)ExtractUserIDResult.MISSING_USER_ID;

            // Get the userID from the token
            var userId = userIdClaim.Value;
            int castToInt;

            // Optionally, log the userID for debugging
            Console.WriteLine("User ID from token: " + userId);


            if (int.TryParse(userId, out castToInt))
                Console.WriteLine(castToInt);
            else
                Console.WriteLine("String could not be parsed.");

            return castToInt;
        }
    }
}