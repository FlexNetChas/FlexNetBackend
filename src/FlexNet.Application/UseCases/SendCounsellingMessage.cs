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
        private readonly ILogger<SendCounsellingMessage> _logger;
        private readonly MessagePersistence _messagePersistence;
        private readonly MessageContextPreparation _contextPreparation;


        public SendCounsellingMessage(
            IGuidanceService guidanceService,
            ILogger<SendCounsellingMessage> logger,
            MessagePersistence messagePersistence,
            MessageContextPreparation messageContextPreparation)
        {
            _guidanceService = guidanceService ?? throw new ArgumentNullException(nameof(guidanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messagePersistence = messagePersistence ?? throw new ArgumentNullException(nameof(messagePersistence));
            _contextPreparation = messageContextPreparation ??
                                  throw new ArgumentNullException(nameof(messageContextPreparation));
        }

        public async Task<SendMessageResponseDto> ExecuteAsync(SendMessageRequestDto request)
        {
            // 1. SETUP
            var context = await _contextPreparation.PrepareAsync(request);

            // 2. AI CALL
            var result = await _guidanceService.GetGuidanceAsync(context.ContextMessage, context.ConversationHistory,
                context.UserContext);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "AI guidance failed for session {SessionId}: {ErrorCode} - {ErrorMessage}",
                    context.Session.Id,
                    result.Error?.ErrorCode,
                    result.Error?.Message);
                
                return new SendMessageResponseDto(
                    Reply: result.Error?.Message ?? "An error occurred",
                    IsSuccess: false,
                    ErrorCode: result.Error?.ErrorCode,
                    CanRetry: result.Error?.CanRetry ?? false,
                    RetryAfter: result.Error?.RetryAfter,
                    SessionId: context.Session.Id!.Value
                );
            }
            // 3. PERSISTENCE
            var safeResponse = await _messagePersistence.SaveMessagesAndGenerateTitleAsync(
                context.Session,
                context.SanitizedMessage,
                result.Data!,
                context.UserContext,
                context.UserId);
            
            // 4. RESPONSE
            return new SendMessageResponseDto(
                Reply: safeResponse,
                IsSuccess: true,
                ErrorCode: null,
                CanRetry: false,
                RetryAfter: null,
                SessionId: context.Session.Id!.Value
            );
        }
    }
} 