using FlexNet.Application.DTOs.Counsellor.Request;
using FlexNet.Application.DTOs.Counsellor.Response;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;

namespace FlexNet.Application.UseCases
{
    public class SendCounsellingMessage
    {
        private readonly IGuidanceService _guidanceService;
        private readonly AiContextBuilder _aiContextBuilder;

        public SendCounsellingMessage(IGuidanceService guidanceService,  AiContextBuilder aiContextBuilder)
        {
            _guidanceService = guidanceService ?? throw new ArgumentNullException(nameof(guidanceService));
            _aiContextBuilder = aiContextBuilder ?? throw new ArgumentNullException(nameof(aiContextBuilder));
        }

        public async Task<SendMessageResponseDto> ExecuteAsync(SendMessageRequestDto request)
        {
            var conversationHistory = Enumerable.Empty<ConversationMessage>();
            var studentContext =
                new StudentContext(Age: request.Age ?? 16, Gender: null, Education: null, Purpose: null);
            
            var contextMessage = _aiContextBuilder.BuildContext(
                studentContext,
                conversationHistory,
                request.Message);
            
            
           var result = await _guidanceService.GetGuidanceAsync(contextMessage, conversationHistory, studentContext);

            if (result.IsSuccess)
            {
                return new SendMessageResponseDto(
                    Reply: result.Data!,
                    IsSuccess: true,
                    ErrorCode: null,
                    CanRetry: false,
                    RetryAfter: null
                );
            }
            else
            {
                return new SendMessageResponseDto(
                    Reply: result.Error?.Message ?? "An error occurred",
                    IsSuccess: false,
                    ErrorCode: result.Error?.ErrorCode,
                    CanRetry: result.Error?.CanRetry ?? false,
                    RetryAfter: result.Error?.RetryAfter
                );
            }
        }
    }




} 