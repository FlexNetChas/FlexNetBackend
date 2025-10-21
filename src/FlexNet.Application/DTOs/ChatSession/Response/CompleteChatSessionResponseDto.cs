using FlexNet.Application.DTOs.ChatMessage.Response;

namespace FlexNet.Application.DTOs.ChatSession.Response
{
    public record CompleteChatSessionResponseDto(
        int Id,
        string? Summary,
        DateTime StartedTime,
        DateTime? EndedTime,
        ICollection<ChatMessageResponseDto> ChatMessages

        // TBD: AI/avatar data stored here too?
);
}