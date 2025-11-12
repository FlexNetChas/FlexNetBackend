using System.Text;
using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.UseCases;

public class SendCounsellingMessageStreaming
{
    private readonly IGuidanceService _guidanceService;
    private readonly MessageContextPreparation _contextPreparation;
    private readonly MessagePersistence _messagePersistence;
    private readonly ILogger<SendCounsellingMessageStreaming> _logger;

    public SendCounsellingMessageStreaming(
        IGuidanceService guidanceService,
        ConversationContextbuilder contextBuilder, MessageContextPreparation contextPreparation, MessagePersistence messagePersistence, ILogger<SendCounsellingMessageStreaming> logger)
    {
        _guidanceService = guidanceService;
        _contextPreparation = contextPreparation;
        _messagePersistence = messagePersistence;
        _logger = logger;
    }

    public async IAsyncEnumerable<Result<string>> ExecuteAsync(SendMessageRequestDto request)
    {
        // 1. SETUP

        var context = await _contextPreparation.PrepareAsync(request);
        
        // 2. BUILD RESPONSE
        var fullResponse = new StringBuilder();

        // 3. AI CALL
        await foreach (var chunk in _guidanceService.GetGuidanceStreamingAsync(
                           context.ContextMessage,
                           context.ConversationHistory,
                           context.UserContext))
        {
            if (!chunk.IsSuccess)
            {
                _logger.LogError(
                    "Streaming failed for session {SessionId}: {ErrorCode} - {ErrorMessage}", 
                    context.Session.Id, 
                    chunk.Error?.ErrorCode, 
                    chunk.Error?.Message);
                yield return chunk;
                yield break;
            }
            // 4. RESPONSE
            fullResponse.Append(chunk.Data);
            yield return chunk;
        }
        // 5. PERSISTENCE
        await _messagePersistence.SaveMessagesAndGenerateTitleAsync(
            context.Session,
            context.SanitizedMessage,
            fullResponse.ToString(),
            context.UserContext,
            context.UserId);
    }
    
}