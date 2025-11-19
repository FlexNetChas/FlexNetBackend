using FlexNet.Application.DTOs.Export;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.UseCases;

public static class UserDataExportMapper
{
    /// <summary>
    ///Converts ChatMessage entity to ChatMessageExportDto
    /// </summary>

    public static ChatMessageExportDto ToExportDto(this ChatMessage entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ChatMessageExportDto(
            MessageText: entity.MessageText,
            Role: entity.Role,
            Timestamp: entity.TimeStamp,
            LastUpdated: entity.LastUpdated
        );
    }

    public static UserExportDto ToExportDto(this User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new UserExportDto(
            FirstName: entity.FirstName,
            LastName: entity.LastName,
            Email: entity.Email,
            Role: entity.Role,
            CreatedAt: entity.CreatedAt,
            IsActive: entity.IsActive);
    }

    public static UserDescriptionExportDto ToExportDto(this UserDescription entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserDescriptionExportDto(
            Age: entity.Age,
            Gender: entity.Gender,
            Education: entity.Education,
            Purpose: entity.Purpose);
    }

    public static ChatSessionExportDto ToExportDto(this ChatSession entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ChatSessionExportDto(
            Summary: entity.Summary,
            StartedTime: entity.StartedTime,
            EndedTime: entity.EndedTime,
            Messages: entity.ChatMessages
                .Select(m => m.ToExportDto())
                .ToList());
    }
}