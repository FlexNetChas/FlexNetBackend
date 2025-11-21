using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;
using FlexNet.Application.Services.Formatters;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Gemini;

public class GuidanceRouter : IGuidanceRouter
{
    private readonly ISchoolService _schoolService;
    private readonly ILogger<GuidanceRouter> _logger;
    private readonly ISchoolSearchDetector _detector;
    private readonly IPromptEnricher _enricher;
    private readonly IAiClient _aiClient;
    private readonly ProgramCatalogBuilder _catalogBuilder;

    private string? _cachedProgramCatalog;

    public GuidanceRouter(ISchoolService schoolService, ILogger<GuidanceRouter> logger, ISchoolSearchDetector detector, IPromptEnricher enricher, IAiClient aiClient, ProgramCatalogBuilder catalogBuilder)
    {
        _schoolService = schoolService ?? throw new ArgumentNullException(nameof(schoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _enricher = enricher ?? throw new ArgumentNullException(nameof(enricher));
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
        _catalogBuilder = catalogBuilder ?? throw new ArgumentNullException(nameof(catalogBuilder));
    }

    public async Task<Result<string>> RouteAndExecuteAsync(string xmlPrompt,
        IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto)
    {
        try
        {
            xmlPrompt = await EnrichPromptWithCatalogAsync(xmlPrompt);

            // 1. Extract raw message 
            var rawMsg = ExtractRawMessage(xmlPrompt);

            // 2. Detect if school related query
            var schoolRequest = _detector.DetectSchoolRequest(rawMsg, conversationHistory);

            if (schoolRequest == null)
                return await _aiClient.CallAsync(xmlPrompt);
            
            // 3. Search skolverket database
            var schools = await SearchSchools(schoolRequest);
            if (schools.Count > 0)
            {
                var enrichedPrompt = _enricher.EnrichWithSchools(xmlPrompt, schools);
                var result = await _aiClient.CallAsync(enrichedPrompt);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to generate school advice: {Error}", result.Error?.Message);
                    return Result<string>.Success(GetSchoolAdviceFallback());
                }
                var formatted = SchoolResponseFormatter.FormatSchoolList(result.Data, schools);
                return Result<string>.Success(formatted);
             
            }
            var noResultsPrompt = _enricher.EnrichWithNoResults(xmlPrompt, schoolRequest);
            var noResultsResponse = await _aiClient.CallAsync(noResultsPrompt);

            if (noResultsResponse.IsSuccess) return noResultsResponse;
            _logger.LogWarning("Failed to generate no-results response: {Error}", noResultsResponse.Error?.Message);
            return Result<string>.Success(GetNoResultsFallback());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RouteAndExecuteAsync");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "GUIDANCE_ERROR",
                Message: $"AI service error: {ex.Message}",
                CanRetry: true,
                RetryAfter: null));
        }
    }

    public async IAsyncEnumerable<Result<string>> RouteAndExecuteStreamingAsync(string xmlPrompt,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        xmlPrompt = await EnrichPromptWithCatalogAsync(xmlPrompt);
        
        var rawMsg = ExtractRawMessage(xmlPrompt);

        var schoolRequest = _detector.DetectSchoolRequest(rawMsg, conversationHistory);

        if (schoolRequest == null)
        {
            await foreach (var chunk in _aiClient.CallStreamingAsync(xmlPrompt))
            {
                yield return chunk;
            } 
            yield break;
        }
     
        var schools = await SearchSchools(schoolRequest);
        if (schools.Count > 0)
        {
            var enrichedPrompt = _enricher.EnrichWithSchools(xmlPrompt, schools);
            var hadSuccessfulChunk = false;

            await foreach (var chunk in _aiClient.CallStreamingAsync(enrichedPrompt))
            {
                if (chunk.IsSuccess)
                {
                    hadSuccessfulChunk = true;
                    yield return chunk;
                }
                else
                {
                    _logger.LogWarning("Streaming error: {Error}", chunk.Error?.Message);
                    if (!hadSuccessfulChunk)
                    {
                        yield return Result<string>.Success(GetSchoolAdviceFallback());
                    }
                    yield break;
                }
            }

            if (!hadSuccessfulChunk) yield break;
            var schoolList = SchoolResponseFormatter.FormatSchoolListOnly(schools);
            yield return Result<string>.Success(schoolList);

            yield break; 
        }
        
        var noResultsPrompt = _enricher.EnrichWithNoResults(xmlPrompt, schoolRequest);
        await foreach (var chunk in _aiClient.CallStreamingAsync(noResultsPrompt))
        {
            yield return chunk;
        }
    }

    private async Task<List<School>> SearchSchools(SchoolRequestInfo request)
    {
        var criteria = new SchoolSearchCriteria(
            Municipality: request.Municipality,
            ProgramCodes: request.ProgramCodes?.AsReadOnly(),
            SearchText: null,
            MaxResult: 5);
        var result = await _schoolService.SearchSchoolsAsync(criteria);

        if (!result.IsSuccess || result.Data == null)
        {
            _logger.LogWarning("Skolverket search failed");
            return [];
        }

        var schools = result.Data.ToList();
        return schools;
    }

    private static string ExtractRawMessage(string msg)
    {
        if (!msg.Contains("<current_message>")) return msg;
        const string startTag = "<current_message>";
        const string endTag = "</current_message>";

        var startIndex = msg.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
        var endIndex = msg.IndexOf(endTag, StringComparison.Ordinal);

        if (startIndex > 0 && endIndex > startIndex) return msg.Substring(startIndex, endIndex - startIndex).Trim();
        return msg;
    }
    private static string GetSchoolAdviceFallback()
    {
        return "Jag har hittat några intressanta skolor åt dig! " +
               "Titta på listan nedan för mer information om varje skola.";
    }

    private static string GetNoResultsFallback()
    {
        return "Tyvärr hittade jag inga skolor som matchar dina kriterier just nu. " +
               "Kan du prova att söka i en närliggande kommun eller överväga relaterade program? " +
               "Jag hjälper gärna till att hitta alternativ!";
    }
    private async Task<string?> GetProgramCatalogAsync()
    {
        // Return cached catalog if available
        if (_cachedProgramCatalog != null)
            return _cachedProgramCatalog;

        // Load and cache
        var result = await _catalogBuilder.BuildCatalogXmlAsync();
    
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
        {
            _cachedProgramCatalog = result.Data;
            _logger.LogInformation("Program catalog loaded and cached");
            return _cachedProgramCatalog;
        }

        _logger.LogWarning("Failed to load program catalog: {Error}", result.Error?.Message);
        return null;
    }
    private async Task<string> EnrichPromptWithCatalogAsync(string xmlPrompt)
    {
        var catalog = await GetProgramCatalogAsync();
    
        if (catalog == null)
            return xmlPrompt;  // Graceful degradation - continue without catalog

        // Insert catalog before the current message
        var catalogSection = $"\n{catalog}\n";
    
        // If there's a <current_message> tag, insert before it
        if (xmlPrompt.Contains("<current_message>"))
        {
            return xmlPrompt.Replace("<current_message>", $"{catalogSection}<current_message>");
        }
    
        // Otherwise, prepend to the entire prompt
        return catalogSection + xmlPrompt;
    }
}