using System.Text;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Exceptions;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services;

public class GeminiGuidanceService : IGuidanceService
{
    private readonly IApiKeyProvider _apiKeyProvider;
    private readonly ISchoolService _schoolService;
    private readonly ILogger<GeminiGuidanceService> _logger;

    public GeminiGuidanceService(
        IApiKeyProvider apiKeyProvider,
        ISchoolService schoolService,
        ILogger<GeminiGuidanceService> logger)
    {
        _apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        _schoolService = schoolService ?? throw new ArgumentNullException(nameof(schoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMessage,
        IEnumerable<ConversationMessage> conversationHistory,
        StudentContext studentContext)
    {
        try
        {
            // Extract raw message from XML context if present
            var rawMessage = ExtractRawMessage(userMessage);
            
            
            // Detect if this is a school-related query
            var schoolRequest = DetectSchoolRequest(rawMessage);
            
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
                    return await BuildSchoolResponse(rawMessage, schools, studentContext);
                }

                return await BuildNoResultsResponse(rawMessage, schoolRequest, studentContext);
            }

            // Regular counseling - no school search needed
            return await GetRegularGuidance(userMessage, studentContext);
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
        StudentContext studentContext)
    {
        var response = new StringBuilder();
        
        // Part 1: AI-generated personalized advice
        
        var aiAdvice = await GetPersonalizedSchoolAdvice(
            userMessage, 
            schools, 
            studentContext);
        
        response.AppendLine("---\n");
        response.AppendLine(aiAdvice); 
        
        // Part 2: School list from Skolverket 
        response.AppendLine("---\n");
        response.AppendLine("**Skolor från Skolverkets officiella register:**\n");
        
        foreach (var school in schools)
        {
            response.AppendLine($"### {school.Name}");
            response.AppendLine($"📍 **Kommun:** {school.Municipality}");
            
            // Programs
            if (school.Programs.Any())
            {
                var programList = string.Join(", ", school.Programs.Take(3).Select(p => p.Name));
                response.AppendLine($"📚 **Program:** {programList}");
            }
            
            // Contact information
            if (!string.IsNullOrEmpty(school.WebsiteUrl))
                response.AppendLine($"🌐 **Webbsida:** {school.WebsiteUrl}");
            
            if (!string.IsNullOrEmpty(school.Phone))
                response.AppendLine($"📞 **Telefon:** {school.Phone}");
            
            if (!string.IsNullOrEmpty(school.Email))
                response.AppendLine($"✉️ **E-post:** {school.Email}");
            
            // Address
            if (school.VisitingAddress != null)
            {
                response.AppendLine($"📍 **Adress:** {school.VisitingAddress.StreetAddress}, " +
                                  $"{school.VisitingAddress.PostalCode} {school.VisitingAddress.Locality}");
            }
            
            response.AppendLine(); 
        }
        _logger.LogInformation("✅ Complete response built: {Chars} characters", response.Length);
        
        return Result<string>.Success(response.ToString());
    }

 
    private async Task<string> GetPersonalizedSchoolAdvice(
        string userMessage,
        List<Domain.Entities.Schools.School> schools,
        StudentContext studentContext)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"En {studentContext.Age}-årig elev frågade: '{userMessage}'");
        prompt.AppendLine();
        prompt.AppendLine($"Jag har visat dem {schools.Count} gymnasieskolor från Skolverkets officiella register:");
        
        // Give AI context about which schools
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
        StudentContext studentContext)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"En {studentContext.Age}-årig elev frågade: '{userMessage}'");
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
        StudentContext studentContext)
    {
        var rawMessage = ExtractRawMessage(userMessage);
    
        // Check if asking about schools but vague
        var isSchoolQuery = new[] { "skola", "gymnasium", "studera", "plugga", "utbildning" }
            .Any(k => rawMessage.ToLowerInvariant().Contains(k));
    
        string prompt;
    
        if (isSchoolQuery)
        {
            // Give AI context to ask the right questions
            prompt = $"""
                      En {studentContext.Age}-årig elev frågade: '{rawMessage}'

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


    private string ExtractRawMessage(string message)
    {
        if (message.Contains("<current_message>"))
        {
            var startTag = "<current_message>";
            var endTag = "</current_message>";
            
            var startIndex = message.IndexOf(startTag) + startTag.Length;
            var endIndex = message.IndexOf(endTag);
            
            if (startIndex > 0 && endIndex > startIndex)
            {
                return message.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        
        return message;
    }


    private SchoolRequestInfo? DetectSchoolRequest(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        // Keywords indicating school search
        var schoolKeywords = new[] 
        { 
            "school", "skola", "skolor", "gymnasium", "gymnasiet",
            "recommend", "rekommendera", "suggest", "föreslå",
            "where", "var", "which", "vilken", "vilka",
            "study", "studera", "plugga", "läsa",
            "show", "visa", "list", "lista",
            "find", "hitta", "search", "sök"
        };
        
        if (!schoolKeywords.Any(keyword => lowerMessage.Contains(keyword)))
            return null;
        
        var request = new SchoolRequestInfo();
        
        // Extract municipality
        var municipalities = new Dictionary<string, string[]>
        {
            ["Stockholm"] = new[] { "stockholm" },
            ["Göteborg"] = new[] { "göteborg", "gothenburg", "goteborg" },
            ["Malmö"] = new[] { "malmö", "malmo" },
            ["Uppsala"] = new[] { "uppsala" },
            ["Lund"] = new[] { "lund" },
            ["Linköping"] = new[] { "linköping", "linkoping" },
            ["Västerås"] = new[] { "västerås", "vasteras" },
            ["Örebro"] = new[] { "örebro", "orebro" },
            ["Norrköping"] = new[] { "norrköping", "norrkoping" },
            ["Helsingborg"] = new[] { "helsingborg" },
            ["Jönköping"] = new[] { "jönköping", "jonkoping" },
            ["Umeå"] = new[] { "umeå", "umea" },
            ["Luleå"] = new[] { "luleå", "lulea" },
            ["Borås"] = new[] { "borås", "boras" },
            ["Eskilstuna"] = new[] { "eskilstuna" },
            ["Gävle"] = new[] { "gävle", "gavle" },
            ["Sundsvall"] = new[] { "sundsvall" },
            ["Södertälje"] = new[] { "södertälje", "sodertalje" }
        };
        
        foreach (var (municipality, variants) in municipalities)
        {
            if (variants.Any(v => lowerMessage.Contains(v)))
            {
                request.Municipality = municipality;
                break;
            }
        }
        
        // Extract program interests
        var programKeywords = new Dictionary<string, string[]>
        {
            ["TE"] = new[] { "technology", "teknik", "tech", "teknikprogrammet" },
            ["NA"] = new[] { "naturvetenskap", "natural science", "naturvetenskapsprogrammet" },
            ["SA"] = new[] { "samhällsvetenskap", "social science", "samhällsvetenskapsprogrammet" },
            ["EK"] = new[] { "ekonomi", "economics", "business", "ekonomiprogrammet" },
            ["ES"] = new[] { "estetisk", "arts", "konst", "musik", "estetiska programmet" },
            ["HU"] = new[] { "humanistisk", "humanities", "humanistiska programmet" },
            ["BA"] = new[] { "barn och fritid", "barn- och fritidsprogrammet" },
            ["BF"] = new[] { "bygg och anläggning", "construction", "bygg- och anläggningsprogrammet" },
            ["EE"] = new[] { "el och energi", "electricity", "el- och energiprogrammet" },
            ["FT"] = new[] { "fordon", "vehicle", "fordonsprogrammet" },
            ["HA"] = new[] { "hantverk", "craft", "hantverksprogrammet" },
            ["HT"] = new[] { "handel och administration", "handels- och administrationsprogrammet" },
            ["IN"] = new[] { "industri", "industrial", "industritekniska programmet" },
            ["RL"] = new[] { "restaurang och livsmedel", "restaurang- och livsmedelsprogrammet" },
            ["VF"] = new[] { "vård och omsorg", "care", "nursing", "vård- och omsorgsprogrammet" }
        };
        
        var detectedPrograms = new List<string>();
        var messageWithSpaces = " " + lowerMessage + " ";
 
        foreach (var (code, keywords) in programKeywords)
        {
            if (keywords.Any(k => messageWithSpaces.Contains(k)))
            {
                detectedPrograms.Add(code);
            }
        }
        
        if (detectedPrograms.Any())
        {
            request.ProgramCodes = detectedPrograms;
        }
        if (request.Municipality == null && (request.ProgramCodes == null || !request.ProgramCodes.Any()))
        {
            _logger.LogInformation("Query too vague - needs location OR program");

            return null;  
        }
        return request;
    }

    /// Helper class for detected school search criteria
   
    private class SchoolRequestInfo
    {
        public string? Municipality { get; set; }
        public List<string>? ProgramCodes { get; set; }
    }
}