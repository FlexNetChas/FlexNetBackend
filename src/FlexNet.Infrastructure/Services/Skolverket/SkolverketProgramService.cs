using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Services.Skolverket.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Skolverket;

public class SkolverketProgramService : IProgramService
{
    private readonly ISkolverketApiClient _apiClient;
    private readonly ILogger<SkolverketProgramService> _logger;
    private readonly IMemoryCache _cache;
    
    private const string CacheKey = "skolverket_all_programs";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(365);

    public SkolverketProgramService(ISkolverketApiClient apiClient, ILogger<SkolverketProgramService> logger,
        IMemoryCache memoryCache)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }
    
    public async Task<Result<IEnumerable<SchoolProgram>>> GetAllProgramsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var programs = await GetProgramsFromCacheOrApiAsync(cancellationToken);
            return Result<IEnumerable<SchoolProgram>>.Success(programs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving programs");
            var error = new ErrorInfo(
                ErrorCode: "PROGRAMS_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to retrieve programs. Please try again.");
            return Result<IEnumerable<SchoolProgram>>.Failure(error);
        }
    }

    public async Task<Result<SchoolProgram>> GetProgramByCodeAsync(string programCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(programCode))
            {
                var error = new ErrorInfo(
                    ErrorCode: "INVALID_CODE",
                    CanRetry: false,
                    RetryAfter: null,
                    Message: "Program code is required");
                return Result<SchoolProgram>.Failure(error);
            }

            var allPrograms = await GetProgramsFromCacheOrApiAsync(cancellationToken);
            var program = allPrograms.FirstOrDefault(p => p.Code.Equals(programCode, StringComparison.InvariantCultureIgnoreCase));

            if (program == null)
            {
                var error = new ErrorInfo(
                    ErrorCode: "PROGRAM_NOT_FOUND",
                    CanRetry: false,
                    RetryAfter: null,
                    Message: $"Program with code {programCode} not found.");
                return Result<SchoolProgram>.Failure(error);
            }
        
            return Result<SchoolProgram>.Success(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program by code {Code}", programCode);
            var error = new ErrorInfo(
                ErrorCode: "FETCH_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to retrieve program details. Please try again.");
            return Result<SchoolProgram>.Failure(error);
        }
    }

    public async Task<Result<int>> RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
        
            _cache.Remove(CacheKey);
        
            var programs = await LoadProgramsFromApiAsync(cancellationToken);
        
            return Result<int>.Success(programs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing program cache");
            var error = new ErrorInfo(
                ErrorCode: "REFRESH_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to refresh program data. Please try again.");
            return Result<int>.Failure(error);
        }
    }
    
    private async Task<List<SchoolProgram>> GetProgramsFromCacheOrApiAsync(
    CancellationToken cancellationToken = default)
{
    if (_cache.TryGetValue(CacheKey, out List<SchoolProgram>? cached) && cached != null)
    {
        return cached;
    }
    
    return await LoadProgramsFromApiAsync(cancellationToken);
}

private async Task<List<SchoolProgram>> LoadProgramsFromApiAsync(
    CancellationToken cancellationToken = default)
{

    var response = await _apiClient.GetProgramsAsync(cancellationToken);
    
    if (response?.Body?.Gy == null || response.Body.Gy.Count == 0)
    {
        _logger.LogWarning("Empty or null response from programs endpoint");
        return new List<SchoolProgram>();
    }

    var programs = response.Body.Gy
        .Select(MapToSchoolProgram)
        .Where(p => p != null)
        .Cast<SchoolProgram>()
        .ToList();

    _cache.Set(CacheKey, programs, CacheDuration);
    
    return programs;
}

private SchoolProgram? MapToSchoolProgram(ProgramDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
    {
        _logger.LogWarning("Program DTO missing required fields");
        return null;
    }

    var studyPaths = dto.StudyPaths
        .Where(sp => !string.IsNullOrWhiteSpace(sp.Code) && !string.IsNullOrWhiteSpace(sp.Name))
        .Select(sp => new StudyPath(sp.Code, sp.Name))
        .ToList();

    return new SchoolProgram(
        Code: dto.Code,
        Name: dto.Name,
        StudyPaths: studyPaths);
}
}