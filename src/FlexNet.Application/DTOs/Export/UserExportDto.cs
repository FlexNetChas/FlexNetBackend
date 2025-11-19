namespace FlexNet.Application.DTOs.Export;
/// <summary>
/// User basic information for GDPR export
/// Excludes: PasswordHash, RefreshTokens (security/system data)
/// </summary>
public record UserExportDto(
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime CreatedAt,
    bool IsActive
);