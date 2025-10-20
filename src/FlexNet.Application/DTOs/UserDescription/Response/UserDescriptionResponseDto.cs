namespace FlexNet.Application.DTOs.UserDescription.Response;

public record UserDescriptionResponseDto(
    int Id,
    int Age,
    string? Gender,
    string Education,
    string Purpose
);