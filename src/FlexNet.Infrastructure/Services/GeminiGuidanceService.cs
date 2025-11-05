using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Exceptions;
using FlexNet.Application.Services;
using FlexNet.Application.Services.Formatters;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services;

public class GeminiGuidanceService : IGuidanceService
{
    private readonly IApiKeyProvider _apiKeyProvider;
    private readonly ISchoolService _schoolService;
    private readonly ILogger<GeminiGuidanceService> _logger;
    private readonly SchoolResponseFormatter _formatter;
    private readonly SchoolSearchDetector _detector;

    public GeminiGuidanceService(
        IApiKeyProvider apiKeyProvider,
        ISchoolService schoolService,
        ILogger<GeminiGuidanceService> logger, 
        SchoolResponseFormatter formatter, 
        SchoolSearchDetector detector)
    {
        _apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        _schoolService = schoolService ?? throw new ArgumentNullException(nameof(schoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMessage,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        try
        {
            // Extract raw message from XML contextDto if present
            var rawMessage = ExtractRawMessage(userMessage);
            
            
            // Detect if this is a school-related query
            var schoolRequest = _detector.DetectSchoolRequest(rawMessage);
            
            if (schoolRequest != null)
            {
                _logger.LogInformation(
                    "🎓 School search detected: Municipality={Mun}, Programs={Prog}",
                    schoolRequest.Municipality ?? "Any",
                    schoolRequest.ProgramCodes != null ? string.Join(",", schoolRequest.ProgramCodes) : "Any");
                
                // Search Skolverket database
                var schools = await SearchSchools(schoolRequest);
                
                if (schools.Any())
                {
                    _logger.LogInformation("✅ Found {Count} schools from Skolverket", schools.Count);
                    
                    // Build response: Skolverket data + AI advice
                    return await BuildSchoolResponse(rawMessage, schools, userContextDto);
                }

                return await BuildNoResultsResponse(rawMessage, schoolRequest, userContextDto);
            }

            // Regular counseling - no school search needed
            return await GetRegularGuidance(userMessage, userContextDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGuidanceAsync");
            throw ServiceException.Unknown($"AI service error: {ex.Message}", ex);
        }
    }

    /// Build response combining AI-generated advice + Skolverket school data
    
    private async Task<Result<string>> BuildSchoolResponse(
        string userMessage,
        List<Domain.Entities.Schools.School> schools,
        UserContextDto userContextDto)
    {
        
        // Part 1: Get AI-generated advice

        var aiAdvice = await GetPersonalizedSchoolAdvice(userMessage, schools, userContextDto);
        
        // Part 2: Format with schools 
        var formattedResponse = _formatter.FormatSchoolList(aiAdvice, schools);

        _logger.LogInformation("✅ Complete response built: {Chars} characters",formattedResponse.Length);
        
        return Result<string>.Success(formattedResponse);
    }

 
    private async Task<string> GetPersonalizedSchoolAdvice(
        string userMessage,
        List<Domain.Entities.Schools.School> schools,
        UserContextDto userContextDto)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"En {userContextDto.Age}-årig elev frågade: '{userMessage}'");
        prompt.AppendLine();
        prompt.AppendLine($"Jag har visat dem {schools.Count} gymnasieskolor från Skolverkets officiella register:");
        
        // Give AI contextDto about which schools
        foreach (var school in schools.Take(3))
        {
            prompt.AppendLine($"- {school.Name} i {school.Municipality}");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("Skriv 3-4 meningar på svenska som:");
        prompt.AppendLine("1. Bekräftar deras intresse för teknik/utbildning");
        prompt.AppendLine("2. Uppmuntrar dem att besöka skolornas webbsidor");
        prompt.AppendLine("3. Föreslår att gå på öppet hus-dagar");
        prompt.AppendLine("4. Erbjuder hjälp med fler frågor");
        prompt.AppendLine();
        prompt.AppendLine("Var varm, stödjande och uppmuntrande.");
        prompt.AppendLine("Skriv ENDAST uppmuntran-texten (inga listor, inga skolnamn).");
        
        try
        {
            var apiKey = await _apiKeyProvider.GetApiKeyAsync();
            var model = new GenerativeModel() { ApiKey = apiKey };
            var response = await model.GenerateContent(prompt.ToString());
            
            return response.Text?.Trim() ?? "Lycka till med ditt val!";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get AI advice, using fallback");
            return "Lycka till med att utforska dessa skolor! Besök deras webbsidor och överväg att gå på öppet hus för att få en känsla för vilken som passar dig bäst.";
        }
    }


    private async Task<Result<string>> BuildNoResultsResponse(
        string userMessage,
        SchoolRequestInfo request,
        UserContextDto userContextDto)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"En {userContextDto.Age}-årig elev frågade: '{userMessage}'");
        prompt.AppendLine();
        prompt.AppendLine("Jag sökte i Skolverkets databas men hittade inga skolor som matchar:");
        
        if (!string.IsNullOrEmpty(request.Municipality))
            prompt.AppendLine($"- Kommun: {request.Municipality}");
        
        if (request.ProgramCodes?.Any() == true)
            prompt.AppendLine($"- Program: {string.Join(", ", request.ProgramCodes)}");
        
        prompt.AppendLine();
        prompt.AppendLine("Hjälp eleven på svenska genom att:");
        prompt.AppendLine("1. Föreslå närliggande kommuner");
        prompt.AppendLine("2. Fråga om de kan överväga relaterade program");
        prompt.AppendLine("3. Uppmuntra dem att specificera sina sökkriterier");
        prompt.AppendLine();
        prompt.AppendLine("Var stödjande och konstruktiv.");
        
        var apiKey = await _apiKeyProvider.GetApiKeyAsync();
        var model = new GenerativeModel() { ApiKey = apiKey };
        var response = await model.GenerateContent(prompt.ToString());
        
        return Result<string>.Success(response.Text);
    }


    private async Task<Result<string>> GetRegularGuidance(
        string userMessage,
        UserContextDto userContextDto)
    {
        var rawMessage = ExtractRawMessage(userMessage);
    
        // Check if asking about schools but vague
        var isSchoolQuery = new[] { "skola", "gymnasium", "studera", "plugga", "utbildning" }
            .Any(k => rawMessage.ToLowerInvariant().Contains(k));
    
        string prompt;
    
        if (isSchoolQuery)
        {
            // Give AI contextDto to ask the right questions
            prompt = $"""
                      En {userContextDto.Age}-årig elev frågade: '{rawMessage}'

                      Eleven är intresserad av gymnasieutbildning men har inte varit specifik ännu.

                      För att kunna söka i skolregistret behöver jag veta:
                      - Vilket ämnesområde/program (t.ex. teknik, naturvetenskap, ekonomi)
                      - Vilken stad/kommun (t.ex. Stockholm, Uppsala, Göteborg)

                      Ställ EN vänlig fråga på svenska för att förstå deras intressen bättre.
                      Var varm och uppmuntrande.
                      """;
        }
        else
        {
            // Regular counseling
            prompt = userMessage;
        }
    
        var apiKey = await _apiKeyProvider.GetApiKeyAsync();
        var model = new GenerativeModel() { ApiKey = apiKey };
        var response = await model.GenerateContent(prompt);
    
        return Result<string>.Success(response.Text);
    }


    private async Task<List<Domain.Entities.Schools.School>> SearchSchools(SchoolRequestInfo request)
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
            return new List<Domain.Entities.Schools.School>();
        }
        var schools = result.Data.ToList();
        return schools;
    }


    private static string ExtractRawMessage(string message)
    {
        if (!message.Contains("<current_message>")) return message;
        var startTag = "<current_message>";
        var endTag = "</current_message>";
            
        var startIndex = message.IndexOf(startTag) + startTag.Length;
        var endIndex = message.IndexOf(endTag);
            
        if (startIndex > 0 && endIndex > startIndex)
        {
            return message.Substring(startIndex, endIndex - startIndex).Trim();
        }

        return message;
    }
    
    public async Task<Result<string>> GenerateTitleAsync(
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto? userContextDto)
    {
        try
        {
            // 1. Build the prompt for title generation
            var prompt = BuildTitlePrompt(conversationHistory);
            _logger.LogInformation("Generating chat title from {Count} messages", conversationHistory.Count());
        
            // 2. Call Gemini API (similar to GetGuidanceAsync)
            var result = await CallGeminiApiAsync(prompt);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("failed to generate title: {Error}", result.Error?.Message);
                return result;
            }

            // 3. Extract and clean the title
            var title = CleanTitle(result.Data);
            _logger.LogInformation("Generated title: '{Title}'", title);

            // 4. Return Result.Success(title)
            return Result<string>.Success(title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat title");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "TITLE_GENERATION_ERROR",
                Message: $"Failed to generate chat title: {ex.Message}",
                CanRetry: false,
                RetryAfter: null));
        }
    }
    private static string CleanTitle(string rawTitle)
    {
        var cleaned = rawTitle.Trim();
    
        // Remove surrounding quotes if present
        if ((cleaned.StartsWith("\"") && cleaned.EndsWith("\"")) ||
            (cleaned.StartsWith("'") && cleaned.EndsWith("'")))
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2).Trim();
        }
    
        // Limit length (safety check)
        if (cleaned.Length > 100)
        {
            cleaned = cleaned.Substring(0, 97) + "...";
        }
    
        return cleaned;
    }
    private static string BuildTitlePrompt(IEnumerable<ConversationMessage> history)
    {
        var sb = new StringBuilder();
    
        sb.AppendLine("Based on the conversation below, generate a short, descriptive title (5-8 words maximum).");
        sb.AppendLine("The title should capture the main topic or purpose of the conversation.");
        sb.AppendLine("Respond with ONLY the title, no quotes, no explanation.");
        sb.AppendLine();
        sb.AppendLine("Conversation:");
    
        foreach (var message in history)
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }
    
        sb.AppendLine();
        sb.AppendLine("Title:");
    
        return sb.ToString();
    }
    
    private async Task<Result<string>> CallGeminiApiAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Calling Gemini API with prompt length: {Length}", prompt.Length);
        
            var apiKey = await _apiKeyProvider.GetApiKeyAsync();
            var model = new GenerativeModel() { ApiKey = apiKey };
            var response = await model.GenerateContent(prompt);
        
            if (string.IsNullOrWhiteSpace(response.Text))
            {
                _logger.LogWarning("Gemini returned empty response");
                return Result<string>.Failure(new ErrorInfo(
                    ErrorCode: "EMPTY_RESPONSE",
                    Message: "AI returned an empty response",
                    CanRetry: false,
                    RetryAfter: null));
            }
        
            _logger.LogDebug("Gemini API call successful, response length: {Length}", response.Text.Length);
            return Result<string>.Success(response.Text.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
            return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "GEMINI_API_ERROR",
                Message: $"Failed to generate content: {ex.Message}",
                CanRetry: true,
                RetryAfter: null));
        }
    }

}