using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Services;
using FlexNet.Domain.Entities;
using Moq;
using Xunit.Abstractions;

namespace FlexNet.Application.Tests;

public class UserDataExportServiceTests
{
    private readonly Mock<IUserDataRepo> _repoMock;
    private readonly UserDataExportService _service;
    private readonly ITestOutputHelper _output;
    
    public UserDataExportServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _repoMock = new Mock<IUserDataRepo>();
        _service = new UserDataExportService(_repoMock.Object);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    public async Task Should_ThrowKeyNotFoundException_WhenUserNotFound(int userId)
    {
        // Arrange
        _repoMock.Setup(r => r.GetCompleteUserDataAsync(userId))
            .ReturnsAsync((User?) null);
        
        // Act
        var exception = await Record.ExceptionAsync(async () => await _service.ExportUserDataAsync(userId));
        
        _output.WriteLine($"Exception thrown: {exception.Message}");
        
        // Assert
        
        Assert.NotNull(exception);
        Assert.IsType<KeyNotFoundException>(exception);
        Assert.Contains($"User with ID {userId} not found", exception.Message);        
    }

    [Theory]
    [InlineData(1, "Rasmus", "Wenngren", "rasmus@example.com")]
    public async Task Should_ReturnCompleteExport_WhenUserExists(
        int userId,
        string firstName,
        string lastName,
        string email)
    {
        
        // Arrange
        var user = new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = "Student",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsActive = true,
            ChatSessions = new List<ChatSession>(),
            UserDescription = null
        };
        
        _repoMock.Setup(r => r.GetCompleteUserDataAsync(userId))
            .ReturnsAsync(user);
        
        // Act
        var result = await _service.ExportUserDataAsync(userId);
        
        _output.WriteLine($"Export completed: {result.User.FirstName} {result.User.LastName}");
        
        // Assert - User data
        Assert.NotNull(result);
        Assert.Equal(firstName, result.User.FirstName);
        Assert.Equal(lastName, result.User.LastName);
        Assert.Equal(email, result.User.Email);
        Assert.Equal("Student", result.User.Role);
        Assert.True(result.User.IsActive);

        // Assert - Nullable fields
        Assert.Null(result.UserDescription);
    
        // Assert - Collections
        Assert.Empty(result.ChatSessions);
    
        // Assert - Statistics (with empty data)
        Assert.Equal(0, result.Statistics.TotalChatSessions);
        Assert.Equal(0, result.Statistics.TotalMessages);
        Assert.Equal(30, result.Statistics.AccountAgeInDays);
    
        // Assert - Export metadata
        Assert.Equal("FlexNet", result.ExportedBy.Platform);
        Assert.Equal("1.0", result.ExportedBy.Version);
        Assert.Contains("GDPR", result.ExportedBy.Reason);
    
        // Assert - Export date is recent
        Assert.True(result.ExportDate <= DateTime.UtcNow);
        Assert.True(result.ExportDate >= DateTime.UtcNow.AddMinutes(-1));
    }
    
}