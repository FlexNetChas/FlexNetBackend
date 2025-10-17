namespace FlexNet.Application.DTOs.User.Request;

public record UpdateUserRequestDto(
    string FirstName,
    string LastName,
    string Email

    // Password and Role will be handle by seperate endpoint for security
);