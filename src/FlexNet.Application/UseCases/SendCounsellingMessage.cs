using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;

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
            var studentContext = new StudentContext(Age: request.Age ?? 16, Gender: null, Education: null, Purpose: null);
            var guidance = await _guidanceService.GetGuidanceAsync(request.Message, conversationHistory, studentContext);
            return new SendMessageResponse { Reply = guidance };
        }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public int? Age { get; set; }  // Optional for now
    }

    public class SendMessageResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}