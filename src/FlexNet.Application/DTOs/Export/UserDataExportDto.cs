namespace FlexNet.Application.DTOs.Export;

/// <summary>
/// Root DTO for GDPR Article 20 - Right to Data Portability
/// Contains all user data in a structured, machine-readable format
/// </summary>
public record UserDataExportDto
(
    DateTime ExportDate, 
    ExportMetadataDto ExportedBy, 
    UserExportDto User,
    UserDescriptionExportDto? UserDescription, 
    List<ChatSessionExportDto> ChatSessions,
    ExportStatisticsDto Statistics 
);