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
        TitleGenerator titleGenerator
    )
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _titleGenerator = titleGenerator ?? throw new ArgumentNullException(nameof(titleGenerator));
    }

    public async Task<Result<string>> GetGuidanceAsync(
        string userMsg,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        return await _router.RouteAndExecuteAsync(userMsg, conversationHistory, userContextDto);
    }

    public async Task<Result<string>> GenerateTitleAsync(
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto? userContextDto)
    {
        // Delegate to TitleGenerator
        return await _titleGenerator.GenerateAsync(conversationHistory, userContextDto);
    }

    public IAsyncEnumerable<Result<string>> GetGuidanceStreamingAsync(string userMsg,
        IEnumerable<ConversationMessage> conversationHistory,
        UserContextDto userContextDto)
    {
        return _router.RouteAndExecuteStreamingAsync(userMsg, conversationHistory, userContextDto);
    }
}