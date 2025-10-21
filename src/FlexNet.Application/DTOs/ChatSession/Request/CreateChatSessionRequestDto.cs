
using FlexNet.Application.DTOs.ChatMessage.Response;

namespace FlexNet.Application.DTOs.ChatSession.Request
{
    public record CreateChatSessionRequestDto(
    int Id,
    string? Summary,
    DateTime StartedTime,
    DateTime? EndedTime,
    ICollection<ChatMessageResponseDto> ChatMessages

// TBD: AI/avatar data stored here too?
);
}

