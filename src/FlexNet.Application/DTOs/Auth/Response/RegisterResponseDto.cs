using FlexNet.Application.DTOs.User;

namespace FlexNet.Application.DTOs.Auth.Response
{
    public record RegisterResponseDto(
        string AccessToken,
        string RefreshToken,
        UserDto User
    );

}
