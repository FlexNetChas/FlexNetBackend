using System.Globalization;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Skolverket;

public class SkolverketSchoolService : ISchoolService
{
    private readonly ISkolverketApiClient _apiClient;
    private readonly ILogger<SkolverketSchoolService> _logger;
    private readonly SkolverketMapper _mapper;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "skolverket_all_schools";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(180);

    //Refactor later to Distributed Cache using SQL-Server
    public SkolverketSchoolService(
        ISkolverketApiClient apiClient,
        ILogger<SkolverketSchoolService> logger,
        IMemoryCache memoryCache,
        SkolverketMapper mapper)
    {
        _apiClient = apiClient ??  throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ??  throw new ArgumentNullException(nameof(logger));
        _cache = memoryCache ??  throw new ArgumentNullException(nameof(memoryCache));
        _mapper = mapper ??  throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<School>>> SearchSchoolsAsync(
        SchoolSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allSchools = await GetAllSchoolsAsync(cancellationToken);
            var filtered = FilterSchools(allSchools, criteria);
            
            return Result<IEnumerable<School>>.Success(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching schools");
            var error = new ErrorInfo(
                ErrorCode: "SEARCH_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to search schools. Please try again.");
            return Result<IEnumerable<School>>.Failure(error);
        }
    }

    public async Task<Result<School>> GetSchoolByCodeAsync(
        string schoolUnitCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(schoolUnitCode))
            {
                var error = new ErrorInfo(
                    ErrorCode: "INVALID_CODE",
                    CanRetry: false,
                    RetryAfter: null,
                    Message: "School unit code is required.");
                return Result<School>.Failure(error);
            }
            
            var allSchools = await GetAllSchoolsAsync(cancellationToken);
            var school = allSchools.FirstOrDefault(s => s.SchoolUnitCode.Equals(schoolUnitCode, StringComparison.OrdinalIgnoreCase));

            if (school != null)
            {
                return Result<School>.Success(school);
            }
            
            school = await FetchSchoolDetailAsync(schoolUnitCode, cancellationToken);

            if (school != null) return Result<School>.Success(school);
            {
                var error = new ErrorInfo(
                    ErrorCode: "SCHOOL_NOT_FOUND",
                    CanRetry: false,
                    RetryAfter: null,
                    Message: $"School with code {schoolUnitCode} not found.");
                return Result<School>.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting school by code {Code}", schoolUnitCode);
            var error = new ErrorInfo(
                ErrorCode: "FETCH_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to retrieve school details. Please try again.");
            return Result<School>.Failure(error);
        }
    }

    public async Task<Result<int>> RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(CacheKey);
            
            var schools = await LoadAllSchoolsFromApiAsync(cancellationToken);
            
            return Result<int>.Success(schools.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache");
            var error = new ErrorInfo(
                ErrorCode: "REFRESH_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to refresh school data. Please try again.");
            return Result<int>.Failure(error);
        } 
    }

    private async Task<List<School>> GetAllSchoolsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out List<School>? cached) && cached != null)
        {
            return cached;
        }
        return await LoadAllSchoolsFromApiAsync(cancellationToken);
    }

    private async Task<List<School>> LoadAllSchoolsFromApiAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        var listResponse = await _apiClient.GetAllGymnasiumSchoolAsync(cancellationToken);
        if (listResponse?.Data?.Attributes == null || listResponse.Data.Attributes.Count == 0)
        {
            _logger.LogWarning("Empty or null responses from list endpoint");
            return new List<School>();
        }

        var schoolCodes = listResponse.Data.Attributes
            .Select(s => s.SchoolUnitCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();


        var fetchTasks = schoolCodes
            .Select(code => FetchSchoolDetailAsync(code, cancellationToken))
            .ToList();

        var schools = await Task.WhenAll(fetchTasks);

        var validSchools = schools
            .Where(s => s != null)
            .Cast<School>()
            .ToList();
        var schoolsWithNullPrograms = validSchools.Where(s => s.Programs == null).ToList();
        if (schoolsWithNullPrograms.Count != 0)
        {
            _logger.LogWarning("Found {Count} schools with null Programs!", schoolsWithNullPrograms.Count);
            foreach (var school in schoolsWithNullPrograms.Take(5))
            {
                _logger.LogWarning("  - {Name} ({Code}) has null Programs", school.Name, school.SchoolUnitCode);
            }
        } 
        var elapsed = DateTime.UtcNow - startTime;
        
        _cache.Set(CacheKey, validSchools, CacheDuration);
        
        return validSchools;
    }

    private async Task<School?> FetchSchoolDetailAsync(string schoolUnitCode,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetSchoolDetailAsync(schoolUnitCode, cancellationToken);

        if (response?.Data == null)
        {
            _logger.LogDebug("No data returned for school {Code}", schoolUnitCode);
            return null;
        }

        var school = _mapper.ToSchool(response.Data);

        if (school == null)
        {
            _logger.LogWarning("Mapper returned null for school {Code}", schoolUnitCode);
        }
        return school;
    }

   private List<School> FilterSchools(List<School> schools, SchoolSearchCriteria criteria)
{
    var query = schools.AsEnumerable();

    // Filter by municipality
    if (!string.IsNullOrWhiteSpace(criteria.Municipality))
    {
        _logger.LogDebug("Filtering by municipality: {Mun}", criteria.Municipality);
        
        query = query.Where(s => 
            !string.IsNullOrEmpty(s.Municipality) &&
            s.Municipality.Equals(criteria.Municipality, StringComparison.OrdinalIgnoreCase));
        
        _logger.LogDebug("After municipality filter: {Count} schools remaining", query.Count());
    }

    // Filter by program codes 
    if (criteria.ProgramCodes?.Any() == true)
    {
        _logger.LogDebug("Filtering by programs: {Programs}", string.Join(",", criteria.ProgramCodes));
        
        query = query.Where(s => 
            s.Programs != null &&  
            s.Programs.Any(p => 
                p != null &&  
                !string.IsNullOrEmpty(p.Code) && 
                criteria.ProgramCodes.Contains(p.Code)));
        
        _logger.LogDebug("After program filter: {Count} schools remaining", query.Count());
    }

    // Filter by search text (school name)
    if (!string.IsNullOrWhiteSpace(criteria.SearchText))
    {
        _logger.LogDebug("Filtering by search text: {Text}", criteria.SearchText);
        
        query = query.Where(s => 
            !string.IsNullOrEmpty(s.Name) &&
            s.Name.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase));
        
        _logger.LogDebug("After search text filter: {Count} schools remaining", query.Count());
    }

    // Limit results
    var result = query.ToList();
    
    if (criteria.MaxResult.HasValue && criteria.MaxResult.Value > 0)
    {
        result = result.Take(criteria.MaxResult.Value).ToList();
    }
    
    _logger.LogDebug("Returning {Count} schools after all filters", result.Count);
    
    return result;
}
}