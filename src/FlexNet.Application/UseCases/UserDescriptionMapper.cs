using FlexNet.Application.DTOs.AI;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.UseCases;

public static class UserDescriptionMapper
{
    public static UserContextDto ToUserContextDto(this UserDescription entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserContextDto(
            Age: entity.Age,
            Gender: entity.Gender,
            Education: entity.Education,
            Purpose: entity.Purpose);
    }
}