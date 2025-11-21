using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Services.Factories;
using FlexNet.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services;

public class MessagePersistence
{
    private readonly IChatSessionRepo _chatSessionRepo;
    private readonly IOutputValidator _outputValidator;
    private readonly ChatMessageCreator _chatMessageCreator;
    private readonly IGuidanceService _guidanceService;
    private readonly ILogger<MessagePersistence> _logger;

    public MessagePersistence(
        IChatSessionRepo chatSessionRepo,
        IOutputValidator outputValidator,
        ChatMessageCreator chatMessageCreator,
        IGuidanceService guidanceService,
        ILogger<MessagePersistence> logger)
    {
        _chatSessionRepo = chatSessionRepo;
        _outputValidator = outputValidator;
        _chatMessageCreator = chatMessageCreator;
        _guidanceService = guidanceService;
        _logger = logger;
    }

    public async Task<string> SaveMessagesAndGenerateTitleAsync(
        ChatSession? session,
        string userMsgText,
        string aiResponseText,
        UserContextDto userContext,
        int userId)
    {
        // 1. Validate AI response safety

        var safeResponse = aiResponseText;
        if (!_outputValidator.IsResponseSafe(aiResponseText))
        {
            _logger.LogWarning(
                "Unsafe AI response detected for user {UserId}, using fallback",
                userId);
            safeResponse = _outputValidator.GetSafeFallbackResponse();
        }

        // 2. Create and save both messages
        if (session != null)
        {
            var userMsg = _chatMessageCreator.Create(
                session.Id!.Value,
                userMsgText,
                MessageRoles.User);

            var aiMessage = _chatMessageCreator.Create(
                session.Id.Value,
                safeResponse,
                MessageRoles.Assistant);
        
            session.ChatMessages.Add(userMsg);
            session.ChatMessages.Add(aiMessage);

            var isFirstExchange = session.ChatMessages.Count == 2;

            await _chatSessionRepo.UpdateAsync(session);
        
            // 3. Generate title if first exchange

            if (!isFirstExchange) return safeResponse;
            var historyForTitle = new List<ConversationMessage>
            {
                new(userMsg.Role, userMsg.MessageText),
                new(aiMessage.Role, aiMessage.MessageText)
            };
            var titleResult = await _guidanceService.GenerateTitleAsync(historyForTitle, userContext);

            if (titleResult.IsSuccess)
            {

                // Reload session to avoid concurrency issues
                session = await _chatSessionRepo.GetByIdAsync(session.Id.Value, userId);
                if (session != null)
                {
                    session.Summary = titleResult.Data;
                    await _chatSessionRepo.UpdateAsync(session);
                }
            }
            else
            {
                _logger.LogWarning("❌ Failed to generate title: {Error}",
                    titleResult.Error?.Message);
            }
        }

        return safeResponse;
    }
}