namespace FlexNet.Application.DTOs.Export;
/// <summary>
/// Chat session with messages for GDPR exprt
/// </summary>
public record ChatSessionExportDto(
   string? Summary,
   DateTime StartedTime,
   DateTime? EndedTime,
   List<ChatMessageExportDto> Messages
);