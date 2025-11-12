using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.UseCases;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services;

public class MessageContextPreparation
{
    private readonly IInputSanitizer _inputSanitizer;
    private readonly IUserContextService _userContextService;
    private readonly IUserDescriptionRepo _userDescriptionRepo;
    private readonly IChatSessionRepo _chatSessionRepo;
    private readonly ConversationContextbuilder _contextBuilder;
    private readonly AiContextBuilder _aiContextBuilder;

    public MessageContextPreparation(
        IInputSanitizer inputSanitizer,
        IUserContextService userContextService,
        IUserDescriptionRepo userDescriptionRepo,
        IChatSessionRepo chatSessionRepo,
        ConversationContextbuilder contextBuilder,
        AiContextBuilder aiContextBuilder)
    {
        _inputSanitizer = inputSanitizer;
        _userContextService = userContextService;
        _userDescriptionRepo = userDescriptionRepo;
        _chatSessionRepo = chatSessionRepo;
        _contextBuilder = contextBuilder;
        _aiContextBuilder = aiContextBuilder;
    }

    public async Task<MessageContext> PrepareAsync(SendMessageRequestDto request)
    {
        // 1. Validate
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("Message cannot be empty. Please provide a message!");
        }

        // 2. Sanitize
        var sanitizedMessage = _inputSanitizer.SanitizeUserInput(request.Message);
        var userId = _userContextService.GetCurrentUserId();

        // 3. Get or create session
        ChatSession? session;
        if (request.ChatSessionId.HasValue)
        {
            session = await _chatSessionRepo.GetByIdAsync(request.ChatSessionId.Value, userId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with id {request.ChatSessionId.Value} was not found!");
            }
        }
        else
        {
            session = new ChatSession
            {
                UserId = userId,
                StartedTime = DateTime.UtcNow,
                ChatMessages = new List<ChatMessage>()
            };
            session = await _chatSessionRepo.AddAsync(session);
        }

        // 4. Get user context
        var userDescription = await _userDescriptionRepo.GetUserDescriptionByUserIdAsync(userId);
        if (userDescription == null)
        {
            throw new InvalidOperationException($"User with id {userId} was not found!");
        }
        var userContext = userDescription.ToUserContextDto();

        // 5. Build conversation history
        var conversationHistory = _contextBuilder.BuildHistory(session.ChatMessages);

        // 6. Build AI context
        var contextMessage = _aiContextBuilder.BuildContext(
            userContext,
            conversationHistory,
            sanitizedMessage);

        return new MessageContext(
            Session: session,
            SanitizedMessage: sanitizedMessage,
            UserId: userId,
            UserContext: userContext,
            ConversationHistory: conversationHistory,
            ContextMessage: contextMessage
        );
    }
}