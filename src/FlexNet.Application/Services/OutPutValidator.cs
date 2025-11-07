using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.Security;

public interface IOutputValidator
{
    bool IsResponseSafe(string aiResponse);
    string GetSafeFallbackResponse();
}

public class OutputValidator : IOutputValidator
{
    private readonly ILogger<OutputValidator> _logger;
    
    // Patterns that indicate AI might have leaked system information
    private static readonly string[] LeakPatterns = new[]
    {
        "<system_instructions>",
        "<student_context>",
        "my system instructions",
        "my original instructions",
        "I was instructed to",
        "my programming",
        "CRITICAL SECURITY RULES"
    };
    
    public OutputValidator(ILogger<OutputValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public bool IsResponseSafe(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return true; // Empty is safe (though not useful)
        
        var lowerResponse = aiResponse.ToLowerInvariant();
        
        foreach (var pattern in LeakPatterns)
        {
            if (lowerResponse.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "AI response contains potential system leak. Pattern: '{Pattern}'",
                    pattern);
                return false;
            }
        }
        
        return true;
    }
    
    public string GetSafeFallbackResponse()
    {
        return "I can't answer that right know. Can I help you with something else?";
    }
}