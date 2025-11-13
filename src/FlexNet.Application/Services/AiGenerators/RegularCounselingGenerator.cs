using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.AiGenerators;

public class RegularCounselingGenerator : IRegularCounselingGenerator
{
    private readonly ILogger<RegularCounselingGenerator> _logger;
    private readonly IAiClient  _aiClient;
    
    public RegularCounselingGenerator(IAiClient aiClient, ILogger<RegularCounselingGenerator> logger)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> GenerateAsync(
        string userMsg,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        try
        {
            // 1. Build prompt
            var prompt = BuildPrompt(userMsg, conversationHistory, userContextDto);
            
            // 2. Call API
            var result = await _aiClient.CallAsync(prompt);
            
            // 3. Handle result
            if (result.IsSuccess) 
                return result;  // No processing needed for regular counseling
            
            // 4. If API fails, use fallback
            _logger.LogWarning("Failed to generate counseling response: {Error}", result.Error?.Message);
            return GetFallbackMessage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating counseling response");
            return GetFallbackMessage();
        }


    }


    public async IAsyncEnumerable<Result<string>> GenerateStreamingAsync(string userMsg,
        IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto)
    {
        var prompt = BuildPrompt(userMsg, conversationHistory, userContextDto);

        await foreach (var chunk in _aiClient.CallStreamingAsync(prompt))
        {
            yield return chunk;
        }
    }
    private static string BuildPrompt(
        string userMsg,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        var prompt = new StringBuilder();
        
        // Add user context
        prompt.AppendLine($"Du är en studie- och yrkesvägledare som hjälper en {userContextDto.Age}-årig elev.");
        prompt.AppendLine();
        
        // Add conversation history if exists
        if (conversationHistory.Any())
        {
            prompt.AppendLine("Tidigare konversation:");
            foreach (var message in conversationHistory.TakeLast(5))  // Last 5 messages for context
            {
                prompt.AppendLine($"{message.Role}: {message.Content}");
            }
            prompt.AppendLine();
        }
        
        // Add current message
        prompt.AppendLine($"Elev: {userMsg}");
        prompt.AppendLine();
        
        // Instructions for AI
        prompt.AppendLine("Svara på svenska som en varm, stödjande och kunnig vägledare.");
        prompt.AppendLine("Ge konkreta råd och uppmuntra eleven att utforska sina intressen.");
        prompt.AppendLine("Håll svaret mellan 3-5 meningar om det inte krävs mer information.");
        
        return prompt.ToString();
    }
    private Result<string> GetFallbackMessage()
    {
        const string fallback = "Tack för din fråga! Jag är här för att hjälpa dig med studier och karriärval. " +
                                "Kan du berätta lite mer om vad du funderar på så kan jag ge bättre vägledning?";

        return Result<string>.Success(fallback);
    }
}