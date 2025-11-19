using FlexNet.Application.DTOs.Export;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.UseCases;

namespace FlexNet.Application.Services;
/// <summary>
/// Service for GDPR Article 20 - Right to Data Portability
/// </summary>
public class UserDataExportService :IUserDataExportService
{
    private readonly IUserDataRepo _repo;

    public UserDataExportService(IUserDataRepo repo)
    {
        _repo = repo;
    }

    public async Task<UserDataExportDto> ExportUserDataAsync(int userId)
    {
        // 1. Get complete user data with all related entitites
        var user = await _repo.GetCompleteUserDataAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found. Cannot export data.");
        }
        
        // 2. Transform entities to DTOs using mappers
        var userDto = user.ToExportDto();
        var userDescriptionDto = user.UserDescription?.ToExportDto();
        var chatSessionsDto = user.ChatSessions
            .Select(s => s.ToExportDto())
            .ToList();
        
        // 3. Calculate statistics
        var statistics = new ExportStatisticsDto(
            TotalChatSessions: user.ChatSessions.Count,
            TotalMessages: user.ChatSessions.SelectMany(s => s.ChatMessages).Count(),
            AccountAgeInDays: (int)(DateTime.UtcNow - user.CreatedAt).TotalDays);
        
        // 4. Build metadata

        var metadata = new ExportMetadataDto(
            Platform: "FlexNet",
            Version: "1.0",
            Reason: "GDPR Article 20 - Right to Data Portability");
        
        // 5. Create and return complete export DTO
        return new UserDataExportDto(
            ExportDate: DateTime.UtcNow,
            ExportedBy: metadata,
            User: userDto,
            UserDescription: userDescriptionDto,
            ChatSessions: chatSessionsDto,
            Statistics: statistics);
    }
}