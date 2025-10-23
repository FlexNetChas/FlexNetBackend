namespace FlexNet.Application.DTOs.UserDescription.Request;

public record PatchUserDescriptionRequestDto(
    int? Age,
    string? Gender,
    string? Education,
    string? Purpose
);