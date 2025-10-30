namespace FlexNet.Application.DTOs.ChatSession.Response
{
    public record CompactChatSessionResponseDto(
        int Id,
        string? Summary,
        DateTime StartedTime,
        DateTime? EndedTime
);
}
