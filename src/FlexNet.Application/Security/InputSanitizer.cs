using System.Text;
using System.Text.RegularExpressions;
using FlexNet.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Security;

public class InputSanitizer : IInputSanitizer
{
    private readonly ILogger<InputSanitizer> _logger;
    private const int MaxInputLength = 2000;
    
    // Patterns that indicate prompt injection attempts
    private static readonly string[] SuspiciousPatterns = new[]
    {
        "ignore previous",
        "ignore all previous",
        "disregard",
        "new instructions",
        "system:",
        "<system>",
        "</system>",
        "you are now",
        "forget everything",
        "act as",
        "pretend you are",
        "roleplay as"
    };
    
    public InputSanitizer(ILogger<InputSanitizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public string SanitizeUserInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        var original = input;
        var sanitized = input;
        
        // 1. Enforce length limit
        if (sanitized.Length > MaxInputLength)
        {
            _logger.LogWarning(
                "Input exceeded max length ({Length} > {Max}). Truncating.",
                sanitized.Length, MaxInputLength);
            sanitized = sanitized.Substring(0, MaxInputLength);
        }
        
        // 2. Check for suspicious patterns (log but don't block)
        DetectSuspiciousContent(sanitized);
        
        // 3. Remove/escape XML tags that could break context structure
        sanitized = EscapeXmlTags(sanitized);
        
        // 4. Remove control characters (except newlines and tabs)
        sanitized = RemoveControlCharacters(sanitized);
        
        // 5. Normalize excessive whitespace
        sanitized = NormalizeWhitespace(sanitized);
        
        // 6. Trim result
        sanitized = sanitized.Trim();
        
        if (sanitized != original)
        {
            _logger.LogDebug("Input was sanitized. Original length: {Original}, Sanitized length: {Sanitized}",
                original.Length, sanitized.Length);
        }
        
        return sanitized;
    }
    
    private void DetectSuspiciousContent(string input)
    {
        var lowerInput = input.ToLowerInvariant();
        
        foreach (var pattern in SuspiciousPatterns)
        {
            if (lowerInput.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Suspicious pattern detected in user input: '{Pattern}'",
                    pattern);
            }
        }
    }
    
    private static string EscapeXmlTags(string input)
    {
        // Replace angle brackets that could form XML tags
        var sb = new StringBuilder(input);
        
        // Replace < and > but preserve HTML entities if they exist
        sb.Replace("<", "&lt;");
        sb.Replace(">", "&gt;");
        
        return sb.ToString();
    }
    
    private static string RemoveControlCharacters(string input)
    {
        // Keep only printable characters, newlines (\n), tabs (\t), and carriage returns (\r)
        return Regex.Replace(input, @"[\p{C}&&[^\r\n\t]]", string.Empty);
    }
    
    private static string NormalizeWhitespace(string input)
    {
        // Replace multiple consecutive spaces with single space
        // Keep newlines intact
        return Regex.Replace(input, @"[ ]{2,}", " ");
    }
}