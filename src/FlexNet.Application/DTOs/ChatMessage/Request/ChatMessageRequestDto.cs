namespace FlexNet.Application.DTOs.ChatMessage.Request
{
    public record ChatMessageRequestDto(
        string MessageText,
        DateTime TimeStamp,
        DateTime? LastUpdated
    );
}
