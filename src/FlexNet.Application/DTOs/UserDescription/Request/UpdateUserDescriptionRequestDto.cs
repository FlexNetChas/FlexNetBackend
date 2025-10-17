namespace FlexNet.Application.DTOs.UserDescription.Request;

public record UpdateUserDescriptionRequestDto(
    int Age,
    string? Gender,
    string Education,
    string Purpose
);