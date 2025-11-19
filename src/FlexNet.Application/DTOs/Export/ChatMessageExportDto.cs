namespace FlexNet.Application.DTOs.Export;
/// <summary>
/// ChatMessage description for GDPR export
/// </summary>
public record ChatMessageExportDto(
    string MessageText,
    string Role,
    DateTime Timestamp,
    DateTime? LastUpdated
);