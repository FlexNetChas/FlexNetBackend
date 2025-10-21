using FlexNet.Application.DTOs.UserDescription.Request;
using FlexNet.Application.DTOs.UserDescription.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services;

public class UserDescriptionService : IUserDescriptionService
{
    private readonly IUserDescriptionRepo _userDescriptionRepo;

    public UserDescriptionService(IUserDescriptionRepo userDescriptionRepo)
    {
        _userDescriptionRepo = userDescriptionRepo;
    }

    public async Task<UserDescriptionResponseDto?> GetUserDescriptionByUserIdAsync(int userId)
    {
        var userDescription = await _userDescriptionRepo.GetUserDescriptionByUserIdAsync(userId);
        
        if (userDescription == null)
        {
            throw new InvalidOperationException("User description not found");
        }

        return MapToDto(userDescription);
    }

    public async Task<UserDescriptionResponseDto> PatchUserDescriptionAsync(int userId, PatchUserDescriptionRequestDto request)
    {
        var userDescription = await _userDescriptionRepo.GetUserDescriptionByUserIdAsync(userId);

        if (userDescription == null)
        {
            throw new InvalidOperationException("User description not found");
        }

        // Only update (PATCH) values that aren't null
        if (request.Age.HasValue)
            userDescription.Age = request.Age.Value;

        if (request.Gender != null)
            userDescription.Gender = request.Gender;

        if (request.Education != null)
            userDescription.Education = request.Education;

        if (request.Purpose != null)
            userDescription.Purpose = request.Purpose;

        var updated = await _userDescriptionRepo.UpdateUserDescriptionAsync(userDescription);
        return MapToDto(updated);
    }

    public UserDescriptionResponseDto MapToDto(UserDescription userDescription)
    {
        return new UserDescriptionResponseDto(
            userDescription.Id,
            userDescription.Age,
            userDescription.Gender,
            userDescription.Education,
            userDescription.Purpose
        );
    }
}