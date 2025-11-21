namespace FlexNet.Application.DTOs.Export;
/// <summary>
/// Userdescription/profile information for GDPR export
/// </summary>
public record UserDescriptionExportDto
(
   int Age,
    string? Gender,
    string Education,
    string Purpose
);