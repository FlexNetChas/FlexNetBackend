namespace FlexNet.Application.DTOs.User.Request;

public record UpdatePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);