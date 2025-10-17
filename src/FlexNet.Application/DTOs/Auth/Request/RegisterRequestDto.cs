namespace FlexNet.Application.DTOs.Auth.Request;

public record RegisterRequestDto(
    string FirstName,
    string LastName,
    string Email,
    string Password

);
