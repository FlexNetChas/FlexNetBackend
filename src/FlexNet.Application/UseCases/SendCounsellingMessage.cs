using FlexNet.Application.DTOs.AI;
using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.DTOs.Counsellor.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;
using FlexNet.Application.Services.Factories;
using FlexNet.Application.Services.Security;
using FlexNet.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.UseCases
{
    public class SendCounsellingMessage
    {
        private readonly IGuidanceService _guidanceService;
        private readonly AiContextBuilder _aiContextBuilder;
        private readonly IChatSessionRepo _chatSessionRepo;
        private readonly IUserDescriptionRepo _userDescriptionRepo;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<SendCounsellingMessage> _logger;
        private readonly IInputSanitizer _inputSanitizer;
        private readonly IOutputValidator _outputValidator;
        private readonly ChatMessageCreator _chatMessageCreator;
        private readonly ConversationContextbuilder _contextBuilder;


        public SendCounsellingMessage(
            IGuidanceService guidanceService,  
            AiContextBuilder aiContextBuilder, 
            IChatSessionRepo chatSessionRepo, 
            IUserDescriptionRepo userDescriptionRepo,
            IUserContextService userContextService, ILogger<SendCounsellingMessage> logger, IInputSanitizer inputSanitizer, IOutputValidator outputValidator, ChatMessageCreator chatMessageCreator, ConversationContextbuilder contextBuilder)
        {
            _guidanceService = guidanceService ?? throw new ArgumentNullException(nameof(guidanceService));
            _aiContextBuilder = aiContextBuilder ?? throw new ArgumentNullException(nameof(aiContextBuilder));
            _chatSessionRepo = chatSessionRepo ?? throw new ArgumentNullException(nameof(chatSessionRepo));
            _userDescriptionRepo = userDescriptionRepo ?? throw new ArgumentNullException(nameof(userDescriptionRepo));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _logger = logger;
            _inputSanitizer = inputSanitizer;
            _outputValidator = outputValidator;
            _chatMessageCreator = chatMessageCreator;
            _contextBuilder = contextBuilder;
        }

        public async Task<SendMessageResponseDto> ExecuteAsync(SendMessageRequestDto request)
        {

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new InvalidOperationException("Message cannot be empty. Please provide a message!");
            }
            var sanitizedMessage = _inputSanitizer.SanitizeUserInput(request.Message);
            var userId = _userContextService.GetCurrentUserId();

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
                session = new ChatSession { UserId = userId, StartedTime = DateTime.UtcNow, ChatMessages = new  List<ChatMessage>() };
                session = await _chatSessionRepo.AddAsync(session);
                
            }

            var userDescription = await _userDescriptionRepo.GetUserDescriptionByUserIdAsync(userId);

            if (userDescription == null) throw new InvalidOperationException($"User with id {userId} was not found!");

            var userContext = userDescription.ToUserContextDto();
            
            var conversationHistory = _contextBuilder.BuildHistory(session.ChatMessages);

    
            var contextMessage = _aiContextBuilder.BuildContext(
                userContext,
                conversationHistory,
                sanitizedMessage);
            
            
           var result = await _guidanceService.GetGuidanceAsync(contextMessage, conversationHistory, userContext);

            if (result.IsSuccess)
            {
                if (!_outputValidator.IsResponseSafe(result.Data!))
                {
                    _logger.LogWarning(
                        "Unsafe AI response detected for user {UserId}, using fallback",
                        userId);
            
                    var fallbackResponse = _outputValidator.GetSafeFallbackResponse();
            
                    // Still save messages but with safe response
               
                    var userChatMessage = _chatMessageCreator.Create(
                        session.Id.Value,
                        sanitizedMessage,
                            MessageRoles.User);

                    var aiChatMessage = _chatMessageCreator.Create(
                        session.Id.Value,
                        sanitizedMessage,
                        MessageRoles.Assistant);
                    session.ChatMessages.Add(userChatMessage);
                    session.ChatMessages.Add(aiChatMessage);
            
                    var totalMessages = session.ChatMessages.Count;
                    var isFirstExchange = (totalMessages == 2);
            
                    await _chatSessionRepo.UpdateAsync(session);
            
                    // Skip title generation for unsafe responses
            
                    return new SendMessageResponseDto(
                        Reply: fallbackResponse,
                        IsSuccess: true,
                        ErrorCode: null,
                        CanRetry: false,
                        RetryAfter: null,
                        SessionId: session.Id.Value
                    );
                }

                var userMessage = _chatMessageCreator.Create(
                    session.Id.Value,
                    sanitizedMessage,
                    MessageRoles.User);

                var aiMessage = _chatMessageCreator.Create(
                    session.Id.Value,
                    result.Data!,
                    MessageRoles.Assistant);
                
                session.ChatMessages.Add(userMessage);
                session.ChatMessages.Add(aiMessage);

                var messageCountBeforeUpdate = session.ChatMessages.Count;

                await _chatSessionRepo.UpdateAsync(session);

                if (messageCountBeforeUpdate != 2)
                    return new SendMessageResponseDto(
                        Reply: result.Data!,
                        IsSuccess: true,
                        ErrorCode: null,
                        CanRetry: false,
                        RetryAfter: null,
                        SessionId: session.Id.Value
                    );
                var historyForTitle = new List<ConversationMessage>
                {
                    new(userMessage.Role, userMessage.MessageText),
                    new(aiMessage.Role, aiMessage.MessageText)
                };

                var titleResult = await _guidanceService.GenerateTitleAsync(
                    historyForTitle, 
                    userContext);

                if (titleResult.IsSuccess)
                {
                    _logger.LogInformation("✅ Title generated: '{Title}'", titleResult.Data);
        
                    session = await _chatSessionRepo.GetByIdAsync(session.Id.Value, userId);
                    session.Summary = titleResult.Data;
                    await _chatSessionRepo.UpdateAsync(session);
        
                    _logger.LogInformation("✅ Session updated with title");
                }
                else
                {
                    _logger.LogWarning("❌ Failed to generate title: {Error}", 
                        titleResult.Error?.Message);
                }

                return new SendMessageResponseDto(
                    Reply: result.Data!,
                    IsSuccess: true,
                    ErrorCode: null,
                    CanRetry: false,
                    RetryAfter: null,
                    SessionId: session.Id.Value
                );
            }
            else
            {
                return new SendMessageResponseDto(
                    Reply: result.Error?.Message ?? "An error occurred",
                    IsSuccess: false,
                    ErrorCode: result.Error?.ErrorCode,
                    CanRetry: result.Error?.CanRetry ?? false,
                    RetryAfter: result.Error?.RetryAfter,
                    SessionId: session.Id.Value
                );
            }
        }
    }




} 