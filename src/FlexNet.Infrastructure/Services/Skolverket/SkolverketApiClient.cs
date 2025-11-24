using System.Net.Http.Json;
using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Services.Skolverket.DTOs;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Skolverket;

public class SkolverketApiClient : ISkolverketApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<SkolverketApiClient> _logger;
    private const string ListEndpoint = "skolenhetsregistret/v2/school-units?school_type=GY&status=AKTIV";
    private const string DetailEndpointTemplate = "skolenhetsregistret/v2/school-units/{0}";
    private const string ProgramsEndpoint = "planned-educations/v4/support/programs";

    public SkolverketApiClient(HttpClient client, ILogger<SkolverketApiClient> logger)
    {
        _client = client ??  throw new ArgumentNullException(nameof(client));
        _logger = logger ??  throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SkolverketListResponse?> GetAllGymnasiumSchoolAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetFromJsonAsync<SkolverketListResponse>(ListEndpoint, cancellationToken);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching school list from Skolverket API");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching school list");
            throw;
        }
    }

    public async Task<SkolverketDetailResponse?> GetSchoolDetailAsync(string schoolUnitCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schoolUnitCode))
        {
            _logger.LogWarning("Attempted to fetch school with null or empty code");
            return null;
        }

        try
        {
            var endpoint = string.Format(DetailEndpointTemplate, schoolUnitCode);

            var response = await _client.GetFromJsonAsync<SkolverketDetailResponse>(endpoint, cancellationToken);

            return response;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("School {Code} not found in Skolverket API", schoolUnitCode);
            return null;  
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error fetching school {Code}", schoolUnitCode);
            return null;  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching school {Code}", schoolUnitCode);
            return null;
        }
    }

    public async Task<SkolverketProgramsResponse?> GetProgramsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetFromJsonAsync<SkolverketProgramsResponse>(ProgramsEndpoint, cancellationToken);
            return response;

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching programs from Skolverket API");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching programs");
            return null;
        }
    }
}