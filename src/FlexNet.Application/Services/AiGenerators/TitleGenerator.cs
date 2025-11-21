using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.AiGenerators;

public class TitleGenerator
{
    private readonly IAiClient  _aiClient;
    private readonly ILogger<TitleGenerator> _logger;

    public TitleGenerator(ILogger<TitleGenerator> logger, IAiClient aiClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aiClient = aiClient ?? throw new  ArgumentNullException(nameof(aiClient));
    }

    public async Task<Result<string>> GenerateAsync(IEnumerable<ConversationMessage> history,
        UserContextDto? userContext)
    {
        try
        {
            // 1. Build prompt
            var prompt = BuildPrompt(history);
            
            // 2. Call API
            var result = await _aiClient.CallAsync(prompt);
            
            // 3. Handle result
            if (result.IsSuccess)return Result<string>.Success(CleanTitle(result.Data ?? "Untitled Conversation"));
            
            // 4. Handle API fail
            _logger.LogWarning("failed to generate title: {Error}", result.Error?.Message);
            return result;
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
    
    private static string BuildPrompt(IEnumerable<ConversationMessage> history)
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
    
    private static string CleanTitle(string rawTitle)
    {
        var cleaned = rawTitle.Trim();
    
        // Remove surrounding quotes if present
        if (("\"".StartsWith(cleaned) && "\"".EndsWith(cleaned)) ||
            ("'".StartsWith(cleaned) && "'".EndsWith(cleaned)))
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2).Trim();
        }
    
        // Limit length (safety check)
        if (cleaned.Length > 100)
        {
            cleaned = string.Concat(cleaned.AsSpan(0, 97), "...");
        }
    
        return cleaned;
    }
    
}