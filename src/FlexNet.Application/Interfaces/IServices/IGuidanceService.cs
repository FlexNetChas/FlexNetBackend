using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IGuidanceService
    {
        Task<Result<string>> GetGuidanceAsync(string userMessage, IEnumerable<ConversationMessage> conversationHistory,
            UserContextDto userContextDto);

        Task<Result<string>> GenerateTitleAsync(IEnumerable<ConversationMessage> conversationHistory, UserContextDto? userContextDto = null);
    }
}