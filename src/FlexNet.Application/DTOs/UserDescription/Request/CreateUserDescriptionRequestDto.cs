namespace FlexNet.Application.DTOs.UserDescription.Request;

public record CreateUserDescriptionRequestDto(
    int Age,
    string? Gender,
    string Education,
    string Purpose
);