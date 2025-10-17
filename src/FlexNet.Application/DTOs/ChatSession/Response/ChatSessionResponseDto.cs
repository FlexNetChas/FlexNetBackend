namespace FlexNet.Application.DTOs.ChatSession.Response;

public record ChatSessionResponseDto(
    int Id,
    string? Summary,
    DateTime StartedTime,
    DateTime? EndedTime
);