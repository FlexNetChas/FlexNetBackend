using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;
using FlexNet.Application.Services.AiGenerators;
using FlexNet.Domain.Entities.Schools;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Gemini;

public class GeminiGuidanceService : IGuidanceService
{
    private readonly ISchoolService _schoolService;
    private readonly ILogger<GeminiGuidanceService> _logger;
    private readonly SchoolSearchDetector _detector;
    private readonly SchoolAdviceGenerator _schoolAdviceGenerator;
    private readonly NoResultsGenerator _noResultsGenerator;
    private readonly RegularCounselingGenerator _regularGenerator;
    private readonly TitleGenerator _titleGenerator;

    public GeminiGuidanceService(
        ISchoolService schoolService,
        ILogger<GeminiGuidanceService> logger,
        SchoolSearchDetector detector,
        SchoolAdviceGenerator schoolAdviceGenerator,
        NoResultsGenerator noResultsGenerator,
        RegularCounselingGenerator regularGenerator,
        TitleGenerator titleGenerator)
    {
        _schoolService = schoolService ?? throw new ArgumentNullException(nameof(schoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _schoolAdviceGenerator =
            schoolAdviceGenerator ?? throw new ArgumentNullException(nameof(schoolAdviceGenerator));
        _noResultsGenerator = noResultsGenerator ?? throw new ArgumentNullException(nameof(noResultsGenerator));
        _regularGenerator = regularGenerator ?? throw new ArgumentNullException(nameof(regularGenerator));
        _titleGenerator = titleGenerator ?? throw new ArgumentNullException(nameof(titleGenerator));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMessage,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        try
        {
            // 1. Extract raw message from XML context if present
            var rawMessage = ExtractRawMessage(userMessage);

            // 2. Detect if this is a school-related query
            var schoolRequest = _detector.DetectSchoolRequest(rawMessage);

            if (schoolRequest != null)
            {
                _logger.LogInformation(
                    "🎓 School search detected: Municipality={Mun}, Programs={Prog}",
                    schoolRequest.Municipality ?? "Any",
                    schoolRequest.ProgramCodes != null ? string.Join(",", schoolRequest.ProgramCodes) : "Any");

                // 3. Search Skolverket database
                var schools = await SearchSchools(schoolRequest);

                if (schools.Any())
                {
                    _logger.LogInformation("✅ Found {Count} schools from Skolverket", schools.Count);
                        
                    // 4a. Delegate to SchoolAdviceGenerator
                    return await _schoolAdviceGenerator.GenerateAdviceAsync(
                        rawMessage, 
                        schools, 
                        userContextDto);
                }

                // 4b. Delegate to NoResultsGenerator
                return await _noResultsGenerator.GenerateAsync(
                    rawMessage, 
                    schoolRequest, 
                    userContextDto);
            }

            // 5. Regular counseling - delegate to RegularCounselingGenerator
            return await _regularGenerator.GenerateAsync(
                userMessage, 
                conversationHistory,
                userContextDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGuidanceAsync");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "GUIDANCE_ERROR",
                Message: $"AI service error: {ex.Message}",
                CanRetry: true,
                RetryAfter: null));
        }
    }

    public async Task<Result<string>> GenerateTitleAsync(
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto? userContextDto)
    {
        // Delegate to TitleGenerator
        return await _titleGenerator.GenerateAsync(conversationHistory, userContextDto);
    }

    private async Task<List<School>> SearchSchools(SchoolRequestInfo request)
    {
        var criteria = new SchoolSearchCriteria(
            Municipality: request.Municipality,
            ProgramCodes: request.ProgramCodes?.AsReadOnly(),
            SearchText: null,
            MaxResult: 5
        );

        var result = await _schoolService.SearchSchoolsAsync(criteria);

        if (!result.IsSuccess || result.Data == null)
        {
            _logger.LogWarning("Skolverket search failed");
            return new List<School>();
        }

        var schools = result.Data.ToList();
        return schools;
    }

    private static string ExtractRawMessage(string message)
    {
        if (!message.Contains("<current_message>")) return message;
        const string startTag = "<current_message>";
        const string endTag = "</current_message>";

        var startIndex = message.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
        var endIndex = message.IndexOf(endTag, StringComparison.Ordinal);

        if (startIndex > 0 && endIndex > startIndex)
        {
            return message.Substring(startIndex, endIndex - startIndex).Trim();
        }

        return message;

    }
}

    


