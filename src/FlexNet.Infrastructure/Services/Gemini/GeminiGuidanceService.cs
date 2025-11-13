using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services.AiGenerators;
using FlexNet.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure.Services.Gemini;

public class GeminiGuidanceService : IGuidanceService
{
    private readonly TitleGenerator _titleGenerator;
    private readonly IGuidanceRouter _router;

    public GeminiGuidanceService(
        IGuidanceRouter router,
        TitleGenerator titleGenerator,
        ILogger<GeminiGuidanceService> logger
    )
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _titleGenerator = titleGenerator ?? throw new ArgumentNullException(nameof(titleGenerator));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMessage,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        return await _router.RouteAndExecuteAsync(userMessage, conversationHistory, userContextDto);
    }

    public async Task<Result<string>> GenerateTitleAsync(
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto? userContextDto)
    {
        // Delegate to TitleGenerator
        return await _titleGenerator.GenerateAsync(conversationHistory, userContextDto);
    }

    public IAsyncEnumerable<Result<string>> GetGuidanceStreamingAsync(string userMessage,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        return _router.RouteAndExecuteStreamingAsync(userMessage, conversationHistory, userContextDto);
    }
}