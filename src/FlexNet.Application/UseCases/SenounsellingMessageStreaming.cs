using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;

namespace FlexNet.Application.UseCases;

public class SendCounsellingMessageStreaming
{
    private readonly IGuidanceService _guidanceService;
    private readonly AiContextBuilder _aiContextBuilder;
    private readonly IUserDescriptionRepo _userDescriptionRepo;
    private readonly IUserContextService _userContextService;
    private readonly IInputSanitizer _inputSanitizer;
    private readonly ConversationContextbuilder _contextBuilder;
    private readonly IChatSessionRepo _chatSessionRepo;

    public SendCounsellingMessageStreaming(
        IGuidanceService guidanceService,
        AiContextBuilder aiContextBuilder,
        IUserDescriptionRepo userDescriptionRepo,
        IUserContextService userContextService,
        IInputSanitizer inputSanitizer,
        ConversationContextbuilder contextBuilder,
        IChatSessionRepo chatSessionRepo)
    {
        _guidanceService = guidanceService;
        _aiContextBuilder = aiContextBuilder;
        _userDescriptionRepo = userDescriptionRepo;
        _userContextService = userContextService;
        _inputSanitizer = inputSanitizer;
        _contextBuilder = contextBuilder;
        _chatSessionRepo = chatSessionRepo;
    }

    public async IAsyncEnumerable<Result<string>> ExecuteAsync(SendMessageRequestDto request)
    {
        // 1. Validate
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            yield return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "INVALID_MESSAGE",
                Message: "Message cannot be empty",
                CanRetry: false,
                RetryAfter: null));
            yield break;
        }

        // 2. Sanitize
        var sanitizedMessage = _inputSanitizer.SanitizeUserInput(request.Message);
        var userId = _userContextService.GetCurrentUserId();

        // 3. Get user context
        var userDescription = await _userDescriptionRepo.GetUserDescriptionByUserIdAsync(userId);
        if (userDescription == null)
        {
            yield return Result<string>.Failure(new ErrorInfo(
                ErrorCode: "USER_NOT_FOUND",
                Message: "User not found",
                CanRetry: false,
                RetryAfter: null));
            yield break;
        }

        var userContext = userDescription.ToUserContextDto();

        // 4. Get conversation history (if session exists)
        var conversationHistory = new List<ConversationMessage>();
        if (request.ChatSessionId.HasValue)
        {
            var session = await _chatSessionRepo.GetByIdAsync(request.ChatSessionId.Value, userId);
            if (session != null)
            {
                conversationHistory = _contextBuilder.BuildHistory(session.ChatMessages);
            }
        }

        // 5. Build AI context
        var contextMessage = _aiContextBuilder.BuildContext(
            userContext,
            conversationHistory,
            sanitizedMessage);

        // 6. STREAM the response (no persistence yet - TODO!)
        await foreach (var chunk in _guidanceService.GetGuidanceStreamingAsync(
            contextMessage,
            conversationHistory,
            userContext))
        {
            yield return chunk;
        }

        // TODO: Add message persistence, session creation, title generation
    }
}