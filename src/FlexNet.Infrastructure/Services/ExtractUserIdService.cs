using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace FlexNet.Infrastructure.Services
{
    public class ExtractUserIdService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExtractUserIdService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /* Retrieve the user ID claim "sub" from JwtRegisteredClaimNames (JwtGenerator.cs)
         * The [Authorize] attribute has already validated the JWT token from the
         * Authorization header (Bearer token) and populated User.Claims.
         *
         * Next JS (Frontend) extracts the token from HttpOnly cookie and send it in the Authorization header
         * This ensures the data is trusted and authenticated */

        public int GetCurrentUserId()
        {
            // Look for the "sub" claim directly (not mapped to NameIdentifier)
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim is null)
                throw new UnauthorizedAccessException("User ID not found in token");

            var userIdValue = userIdClaim.Value;

            if (!int.TryParse(userIdValue, out int userId))
                throw new UnauthorizedAccessException("Invalid User ID format in token");

            return userId;
        }

        //public int GetCurrentUserId()
        //{
        //    // Extract the JWT token from the session cookie
        //    var token = _httpContextAccessor.HttpContext?.Request.Cookies["session"];

        //    if (string.IsNullOrEmpty(token))
        //        throw new UnauthorizedAccessException("Token is missing");

        //    // Decode the JWT token to extract the userID
        //    var handler = new JwtSecurityTokenHandler();
        //    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        //    var userIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub");

        //    if (userIdClaim is null)
        //        throw new UnauthorizedAccessException("User ID is missing in the token");

        //    // Get the userID from the token
        //    var userId = userIdClaim.Value;
        //    int castToInt;

        //    // Optionally, log the userID for debugging
        //    Console.WriteLine("User ID from token: " + userId);

        //    if (int.TryParse(userId, out castToInt))
        //        Console.WriteLine(castToInt);
        //    else
        //        Console.WriteLine("String could not be parsed.");

        //    return castToInt;
        //}
    }
}