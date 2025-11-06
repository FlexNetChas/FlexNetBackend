using FlexNet.Application.Models;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services;

public class ConversationContextbuilder
{
    public List<ConversationMessage> BuildHistory(ICollection<ChatMessage> messages)
    {
       return messages
           .OrderBy(m => m.TimeStamp)
           .TakeLast(10)
           .Select(m => new ConversationMessage(Role: m.Role, Content: m.MessageText))
           .ToList();
    }    
}