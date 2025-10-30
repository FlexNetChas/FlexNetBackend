using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;
using Microsoft.AspNetCore.Mvc;

namespace FlexNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolController : ControllerBase
{
    private readonly ISchoolService _schoolService;
    private readonly ILogger<SchoolController> _logger;
    
    public SchoolController(
        ISchoolService schoolService,
        ILogger<SchoolController> logger)
    {
        _schoolService = schoolService;
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "OK",
            message = "School service is registered and controller is working",
            timestamp = DateTime.UtcNow
        });
    }
 
    [HttpPost("search")]
    [ProducesResponseType(typeof(IEnumerable<School>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchSchools(
        [FromBody] SearchSchoolsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var criteria = new SchoolSearchCriteria(
                Municipality: request.Municipality,
                ProgramCodes: request.ProgramCodes?.AsReadOnly(),
              
                MaxResult: request.MaxResults
            );
            
            var result = await _schoolService.SearchSchoolsAsync(criteria, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return StatusCode(500, new
                {
                    error = result.Error?.Message,
                    errorCode = result.Error?.ErrorCode,
                    canRetry = result.Error?.CanRetry,
                    retryAfter = result.Error?.RetryAfter
                });
            }
            
            return Ok(result.Data);  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching schools");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(School), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchoolByCode(
        string code,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schoolService.GetSchoolByCodeAsync(code, cancellationToken);
            
            if (!result.IsSuccess)
            {
                if (result.Error?.ErrorCode == "SCHOOL_NOT_FOUND")
                    return NotFound(new { message = result.Error.Message });
                
                return StatusCode(500, new { error = result.Error?.Message });
            }
            
            return Ok(result.Data);  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting school {Code}", code);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    

    [HttpPost("refresh-cache")]
    public async Task<IActionResult> RefreshCache(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schoolService.RefreshCacheAsync(cancellationToken);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { error = result.Error?.Message });
            
            return Ok(new { message = "Cache refreshed", schoolCount = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
}


public record SearchSchoolsRequest(
    string? Municipality = null,
    List<string>? ProgramCodes = null,
    string? SearchText = null,
    int? MaxResults = 100
);