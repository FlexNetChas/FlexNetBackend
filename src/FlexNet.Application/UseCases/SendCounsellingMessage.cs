using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.UseCases
{
    public class SendCounsellingMessage
    {
        private readonly IGuidanceService _guidanceService;

        public SendCounsellingMessage(IGuidanceService guidanceService)
        {
            _guidanceService = guidanceService ?? throw new ArgumentNullException(nameof(guidanceService));
        }

        public async Task<SendMessageResponse> ExecuteAsync(SendMessageRequest request)
        {
            var conversationHistory = Enumerable.Empty<ConversationMessage>();
            var studentContext =
                new StudentContext(Age: request.Age ?? 16, Gender: null, Education: null, Purpose: null);
           var result = await _guidanceService.GetGuidanceAsync(request.Message, conversationHistory, studentContext);

           if (result.IsSuccess)
           {
               return new SendMessageResponse
               {
                   Reply = result.Content,
                   IsSuccess = true,
                   ErrorCode = null,
                   CanRetry = false,
                   RetryAfter = null
               };
               
           }
           else
           {
               return new SendMessageResponse
               {
                   Reply = result.Error?.Message ?? "An error occurred",
                   IsSuccess = false,
                   ErrorCode = result.Error?.ErrorCode,
                   CanRetry = result.Error?.CanRetry ?? false,
                   RetryAfter = result.Error?.RetryAfter
               };
           }
        }
    }




} 