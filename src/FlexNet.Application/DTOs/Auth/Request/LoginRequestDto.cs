namespace FlexNet.Application.DTOs.Auth.Request;

public record LoginRequestDto(
    string Email,
    string Password
);
