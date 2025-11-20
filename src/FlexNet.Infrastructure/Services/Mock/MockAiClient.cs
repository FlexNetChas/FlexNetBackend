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
    
        var delay = _random.Next(_minDelayMs, _maxDelayMs);
        await Task.Delay(delay);
    
        // Check if it's a title request
        string response;
        if (IsTitleRequest(prompt))
        {
            response = GenerateRandomTitle();
            _logger.LogInformation("🎭 MOCK: Generated title ({Chars} chars) after {Delay}ms", 
                response.Length, delay);
        }
        else
        {
            response = GenerateRandomResponse();
            _logger.LogInformation("🎭 MOCK: Generated counseling response ({Chars} chars) after {Delay}ms", 
                response.Length, delay);
        }
    
        return Result<string>.Success(response);
    }

    public async IAsyncEnumerable<Result<string>> CallStreamingAsync(string prompt)
    {
        _logger.LogInformation("🎭 MOCK: Streaming AI call (prompt: {Length} chars)", prompt.Length);

        // Simulate delay before first chunk
        await Task.Delay(_random.Next(_minDelayMs, _maxDelayMs));

        // Check if it's a title request (titles don't need streaming)
        if (IsTitleRequest(prompt))
        {
            var title = GenerateRandomTitle();
            _logger.LogInformation("🎭 MOCK: Generated title for streaming");
            yield return Result<string>.Success(title);
            yield break;
        }

        // Generate full response
        var fullResponse = GenerateRandomResponse();
        _logger.LogInformation("🎭 MOCK: Streaming counseling response ({Chars} chars)", fullResponse.Length);

        // Split into chunks (simulate streaming)
        var words = fullResponse.Split(' ');
        var chunkSize = 3; // 3 words per chunk

        for (int i = 0; i < words.Length; i += chunkSize)
        {
            var chunk = string.Join(" ", words.Skip(i).Take(chunkSize));
    
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                yield return Result<string>.Success(chunk + " ");
        
                // Small delay between chunks to simulate real streaming
                await Task.Delay(_random.Next(50, 150));
            }
        }

        _logger.LogInformation("🎭 MOCK: Streaming completed");
    }

    private bool IsTitleRequest(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        return lower.Contains("title") && lower.Contains("conversation");
    }

    private string GenerateRandomTitle()
    {
        var titles = new[]
        {
            "Studievägledning",
            "Karriärfrågor",
            "Utbildningsval",
            "Gymnasievägledning",
            "Framtidsplanering"
        };
    
        return titles[_random.Next(titles.Length)];
    }

    private string GenerateRandomResponse()
    {
        var responses = new[]
        {
            "Det låter spännande! Berätta gärna mer om dina intressen så kan jag hjälpa dig hitta rätt utbildning.",
            "Intressant val! Vilka ämnen tycker du är roligast i skolan idag?",
            "Bra att du tänker på din framtid! Finns det någon specifik stad du är intresserad av att studera i?",
            "Coolt! Vad är det som gör att du är intresserad av just det här området?",
            "Jag hjälper dig gärna! Har du funderat på om du vill gå teoretiska eller praktiska program?",
            "Spännande! Känner du till vilka gymnasieprogram som finns inom det området?"
        };
    
        return responses[_random.Next(responses.Length)];
    }
}