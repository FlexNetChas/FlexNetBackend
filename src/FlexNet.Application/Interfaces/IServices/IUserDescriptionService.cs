using FlexNet.Application.DTOs.UserDescription.Request;
using FlexNet.Application.DTOs.UserDescription.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices;

public interface IUserDescriptionService
{
    Task<UserDescriptionResponseDto?> GetUserDescriptionByUserIdAsync(int userId);
    Task<UserDescriptionResponseDto> PatchUserDescriptionAsync(int userId, PatchUserDescriptionRequestDto request);

    // Simple mapper for UserDescription Entity to UserDescriptionResponseDto
    UserDescriptionResponseDto MapToDto(UserDescription userDescription);
}