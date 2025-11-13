using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Gemini;

public class GuidanceRouter : IGuidanceRouter
{
    private readonly ISchoolService _schoolService;
    private readonly ILogger<GuidanceRouter> _logger;
    private readonly ISchoolSearchDetector _detector;
    private readonly ISchoolAdviceGenerator _adviceGenerator;
    private readonly INoResultsGenerator _noResultsGenerator;
    private readonly IRegularCounselingGenerator _regularGenerator;

    public GuidanceRouter(ISchoolService schoolService, ILogger<GuidanceRouter> logger, ISchoolSearchDetector detector,
        ISchoolAdviceGenerator adviceGenerator, INoResultsGenerator noResultsGenerator,
        IRegularCounselingGenerator regularGenerator)
    {
        _schoolService = schoolService ?? throw new ArgumentNullException(nameof(schoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _adviceGenerator = adviceGenerator ?? throw new ArgumentNullException(nameof(adviceGenerator));
        _noResultsGenerator = noResultsGenerator ?? throw new ArgumentNullException(nameof(noResultsGenerator));
        _regularGenerator = regularGenerator ?? throw new ArgumentNullException(nameof(regularGenerator));
    }

    public async Task<Result<string>> RouteAndExecuteAsync(string userMsg,
        IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto)
    {
        try
        {
            // 1. Extract raw message 
            var rawMsg = ExtractRawMessage(userMsg);

            // 2. Detect if school related query
            var schoolRequest = _detector.DetectSchoolRequest(rawMsg);

            if (schoolRequest == null)
                return await _regularGenerator.GenerateAsync(userMsg, conversationHistory, userContextDto);
            
            // 3. Search skolverket database
            var schools = await SearchSchools(schoolRequest);
            if (schools.Count != 0)
            {
                // 4a. Delegate to AdviceGenerator
                return await _adviceGenerator.GenerateAdviceAsync(rawMsg, schools, userContextDto);
            }

            // 4b. Delegate to NoResultGenerator
            return await _noResultsGenerator.GenerateAsync(rawMsg, schoolRequest, userContextDto);


            // 5. Regular counseling - delegate to RegularGenerator
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

    public async IAsyncEnumerable<Result<string>> RouteAndExecuteStreamingAsync(string userMsg,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        var rawMsg = ExtractRawMessage(userMsg);

        var schoolRequest = _detector.DetectSchoolRequest(rawMsg);
        if (schoolRequest != null)
        {
            var result = await RouteAndExecuteAsync(userMsg, conversationHistory, userContextDto);
            yield return result;
            yield break;
        }

        await foreach (var chunk in _regularGenerator.GenerateStreamingAsync(userMsg, conversationHistory,
                           userContextDto))
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
}