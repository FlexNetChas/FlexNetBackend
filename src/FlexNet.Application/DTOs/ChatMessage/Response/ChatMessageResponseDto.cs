namespace FlexNet.Application.DTOs.ChatMessage.Response;

public record ChatMessageResponseDto(
    int Id,
    string MessageText,
    DateTime TimeStamp,
    DateTime? LastUpdated
);