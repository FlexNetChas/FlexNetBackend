using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories;

/// <summary>
/// Repository for GDPR data export operations
/// </summary>
public interface IUserDataRepo
{
    /// <summary>
    /// Gets complete user data including all related entities for GDPR export
    /// </summary>
    /// <param name="userId">The user ID to export data for</param>
    /// <returns>User with all navigation properties loaded, or null if not found</returns>
    Task<User?> GetCompleteUserDataAsync(int userId);
}