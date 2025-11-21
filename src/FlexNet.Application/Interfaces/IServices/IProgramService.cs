using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Interfaces.IServices;
/// <summary>
/// Service for retrieving gymnasium program data from Skolverket.
/// Provides cached access to program information.
/// </summary>
public interface IProgramService
{
    /// <summary>
    /// Retrieves all available gymnasium programs.
    /// Results are cached for performance.
    /// </summary>
    Task<Result<IEnumerable<SchoolProgram>>> GetAllProgramsAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a specific program by its code (e.g., "BA25", "TE25").
    /// </summary>
    Task<Result<SchoolProgram>> GetProgramByCodeAsync(
        string programCode,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Manually refreshes the program cache.
    /// Returns the number of programs loaded.
    /// </summary>
    Task<Result<int>> RefreshCacheAsync(
        CancellationToken cancellationToken = default); 
}