using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IGuidanceService
    {
        Task<GuidanceResult> GetGuidanceAsync(string userMessage, IEnumerable<ConversationMessage> conversationHistory,
            StudentContext studentContext);
    }
}