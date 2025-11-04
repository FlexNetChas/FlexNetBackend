namespace FlexNet.Application.DTOs.AI
{
    public record UserContextDto(
        int Age,
        string? Gender,
        string? Education,
        string? Purpose);
}