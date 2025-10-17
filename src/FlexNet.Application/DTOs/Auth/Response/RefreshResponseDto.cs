namespace FlexNet.Application.DTOs.Auth.Response
{
    public record RefreshResponseDto(
        string AccessToken,
        string RefreshToken
    );
}