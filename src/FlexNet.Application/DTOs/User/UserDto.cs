
namespace FlexNet.Application.DTOs.User
{
    public record UserDto(
        int Id,
        string FirstName,
        string LastName,
        string Email,
        string Role
    );
}
