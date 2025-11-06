using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services.Factories;

public class ChatMessageCreator
{
    public ChatMessage Create(int sessionId, string messageText, string role)
    {
        return new ChatMessage
        {
            TimeStamp = DateTime.UtcNow,
            ChatSessionId = sessionId,
            MessageText = messageText,
            Role = role
        };
    }    
}