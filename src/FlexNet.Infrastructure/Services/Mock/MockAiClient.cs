using System.Text;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Mock;

/// Mock AI client for development - no rate limits, no real API calls!
/// Returns realistic Swedish responses based on prompt type detection.
public class MockAiClient : IAiClient
{
    private readonly ILogger<MockAiClient> _logger;
    private readonly Random _random = new();
    
    // Simulate network delay (adjustable)
    private readonly int _minDelayMs;
    private readonly int _maxDelayMs;

    public MockAiClient(ILogger<MockAiClient> logger, int minDelayMs = 100, int maxDelayMs = 500)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _minDelayMs = minDelayMs;
        _maxDelayMs = maxDelayMs;
    }

    public async Task<Result<string>> CallAsync(string prompt)
    {
        _logger.LogInformation("🎭 MOCK: AI call intercepted (no real API call made)");
        _logger.LogDebug("Mock prompt length: {Length} characters", prompt.Length);
        
        // Simulate network delay (makes dev feel more realistic)
        var delay = _random.Next(_minDelayMs, _maxDelayMs);
        await Task.Delay(delay);
        
        // Detect prompt type and return appropriate response
        var promptType = DetectPromptType(prompt);
        var response = GenerateResponse(promptType, prompt);
        
        _logger.LogInformation("🎭 MOCK: Returned {Type} response ({Chars} chars) after {Delay}ms", 
            promptType, response.Length, delay);
        
        return Result<string>.Success(response);
    }

    private PromptType DetectPromptType(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        
        // Title generation prompt
        if (lower.Contains("title") && lower.Contains("conversation"))
            return PromptType.Title;
        
        // School advice prompt
        if (lower.Contains("gymnasieskolor") && lower.Contains("skolverkets"))
            return PromptType.SchoolAdvice;
        
        // No results prompt
        if (lower.Contains("hittade inga skolor"))
            return PromptType.NoResults;
        
        // Regular counseling
        return PromptType.RegularCounseling;
    }

    private string GenerateResponse(PromptType type, string prompt)
    {
        return type switch
        {
            PromptType.Title => GenerateTitleResponse(),
            PromptType.SchoolAdvice => GenerateSchoolAdviceResponse(prompt),
            PromptType.NoResults => GenerateNoResultsResponse(prompt),
            PromptType.RegularCounseling => GenerateRegularCounselingResponse(prompt),
            _ => "Tack för din fråga! Jag är här för att hjälpa dig."
        };
    }

    private string GenerateTitleResponse()
    {
        // Vary responses to make it feel realistic
        var titles = new[]
        {
            "Studievägledning och Karriärval",
            "Gymnasieval och Utbildning",
            "Hjälp med Skolval",
            "Studie- och Yrkesvägledning",
            "Vägledning för Gymnasievalet"
        };
        
        return titles[_random.Next(titles.Length)];
    }

    private string GenerateSchoolAdviceResponse(string prompt)
    {
        // Extract age if present (for more realistic response)
        var ageMatch = System.Text.RegularExpressions.Regex.Match(prompt, @"(\d+)-årig");
        var age = ageMatch.Success ? ageMatch.Groups[1].Value : "ung";
        
        var responses = new[]
        {
            $"Det är fantastiskt att du är intresserad av att söka till gymnasiet! " +
            $"De skolor jag har visat dig erbjuder alla utmärkta program som passar dina intressen. " +
            $"Jag rekommenderar starkt att du besöker deras webbplatser för att lära dig mer om varje skola. " +
            $"Öppet hus-dagar är också ett perfekt tillfälle att få känslan för skolmiljön och träffa lärare. " +
            $"Tveka inte att ställa fler frågor om du behöver mer hjälp!",
            
            $"Vilken spännande tid i ditt liv! Att välja gymnasieskola är ett viktigt steg. " +
            $"Skolorna jag har hittat åt dig har alla starka program inom ditt intresseområde. " +
            $"Ta dig tid att utforska deras webbsidor och läs om de olika programmen. " +
            $"Jag föreslår också att du går på öppet hus så du kan se skolorna med egna ögon. " +
            $"Kommer du på fler frågor? Jag hjälper gärna till!",
            
            $"Jag ser att du funderar på gymnasievalet - det är jättebra att du planerar framåt! " +
            $"De skolor som visas erbjuder program som verkar passa dig väl. " +
            $"Besök gärna deras hemsidor för mer detaljerad information om utbildningarna. " +
            $"Många skolor har öppet hus där du kan ställa frågor direkt till lärare och elever. " +
            $"Hör av dig om du vill veta mer om något!"
        };
        
        return responses[_random.Next(responses.Length)];
    }

    private string GenerateNoResultsResponse(string prompt)
    {
        // Check if municipality or program was mentioned
        var hasMunicipality = prompt.Contains("Kommun:");
        var hasProgram = prompt.Contains("Program:");
        
        var sb = new StringBuilder();
        
        sb.AppendLine("Tyvärr hittade jag inga skolor som exakt matchar dina kriterier just nu.");
        sb.AppendLine();
        
        if (hasMunicipality)
        {
            sb.AppendLine("Några förslag:");
            sb.AppendLine("• Prova att söka i närliggande kommuner - ibland finns utmärkta skolor bara en kommun bort");
        }
        
        if (hasProgram)
        {
            sb.AppendLine("• Överväg relaterade program som kan ge liknande kompetenser");
        }
        
        sb.AppendLine("• Specificera dina intressen mer - ju mer jag vet, desto bättre kan jag hjälpa dig hitta rätt skola!");
        sb.AppendLine();
        sb.AppendLine("Berätta gärna mer om vad du är intresserad av, så kan vi söka tillsammans!");
        
        return sb.ToString().Trim();
    }

    private string GenerateRegularCounselingResponse(string prompt)
    {
        // Extract if it's about school/education
        var lower = prompt.ToLowerInvariant();
        var isSchoolRelated = new[] { "skola", "studera", "utbildning", "gymnasium", "plugga" }
            .Any(keyword => lower.Contains(keyword));
        
        if (isSchoolRelated)
        {
            return "Det låter som en viktig fråga om din framtid! " +
                   "Jag hjälper gärna till med studievägledning. " +
                   "Kan du berätta lite mer om vad du funderar på? " +
                   "Till exempel vilket ämnesområde du är intresserad av, eller vilken stad du helst vill studera i?";
        }
        
        // Generic counseling response
        var responses = new[]
        {
            "Tack för att du delar dina tankar med mig! " +
            "Jag är här för att hjälpa dig navigera dina val. " +
            "Kan du berätta lite mer så vi kan utforska dina alternativ tillsammans?",
            
            "Det är bra att du funderar på din framtid! " +
            "Låt oss prata om vad som intresserar dig mest. " +
            "Finns det något särskilt område eller ämne som du tycker verkar spännande?",
            
            "Jag uppskattar att du kom till mig med detta! " +
            "Studie- och karriärval kan kännas överväldigande, men vi tar det steg för steg. " +
            "Vad är det som får dig att fundera just nu?"
        };
        
        return responses[_random.Next(responses.Length)];
    }

    private enum PromptType
    {
        Title,
        SchoolAdvice,
        NoResults,
        RegularCounseling
    }
}