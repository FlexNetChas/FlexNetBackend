using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Models;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IGuidanceService
    {
        Task<string> GetGuidanceAsync(string userMessage, IEnumerable<ConversationMessage> conversationHistory, StudentContext studentContext);
    }
}