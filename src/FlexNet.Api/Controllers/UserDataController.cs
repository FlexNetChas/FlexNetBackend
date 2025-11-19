using System.Text;
using System.Text.Json;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlexNet.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("global-quota")]
public class UserDataController : ControllerBase
{
    private readonly IUserDataExportService _export;
    private readonly IUserContextService _context;
    private readonly ILogger<UserDataController> _logger;

    public UserDataController(IUserDataExportService export, IUserContextService context,
        ILogger<UserDataController> logger)
    {
        _export = export;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GDPR Article 20 - Right to Data Portability
    /// Exports all user data in machine-readable JSON format
    /// </summary>
    /// <returns>JSON file download with complete user data</returns>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportUserData()
    {
        try
        {
            // Get current user ID from JWT token
            var userId = _context.GetCurrentUserId();

            // Get complete user data export
            var exportData = await _export.ExportUserDataAsync(userId);

            // Serialize to JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var jsonString = JsonSerializer.Serialize(exportData, jsonOptions);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            // Generate filename with timestamp
            var fileName = $"flexnet-userdata-{DateTime.UtcNow:yyyy-MM-dd}.json";

            // Return as downlodable file
            return File(
                fileContents: jsonBytes,
                contentType: "application/json",
                fileDownloadName: fileName);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found for GDPR export");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GDPR export");
            return StatusCode(500, new { message = "An error occured during data export"});
        }
    }
}