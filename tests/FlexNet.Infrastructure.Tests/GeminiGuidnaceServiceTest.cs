using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services.AiGenerators;
using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Services.Gemini;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace FlexNet.Infrastructure.Tests;

public class GeminiGuidanceServiceTest
{
    private readonly Mock<IGuidanceRouter> _routerMock;
    private readonly Mock<ILogger<GeminiGuidanceService>> _loggerMock;
    private readonly GeminiGuidanceService _service;
    private readonly ITestOutputHelper _output;

    public GeminiGuidanceServiceTest(ITestOutputHelper output)
    {
        _routerMock = new Mock<IGuidanceRouter>();
        _loggerMock = new Mock<ILogger<GeminiGuidanceService>>();
    
        // Create real TitleGenerator with mocked dependencies
        var titleLoggerMock = new Mock<ILogger<TitleGenerator>>();
        var aiClientMock = new Mock<IAiClient>();
        var titleGenerator = new TitleGenerator(titleLoggerMock.Object, aiClientMock.Object);
    
        _service = new GeminiGuidanceService(
            _routerMock.Object,
            titleGenerator
        );
    
        _output = output;
    }

    [Fact]
    public async Task Should_ReturnResult_WhenRouterSucceeds()
    {
        // Arrange

        var userMsg = "Hello, I want to study to becoma a doctor. What kind of programs should I study?";
        var conversationHistory = Enumerable.Empty<ConversationMessage>();
        var context = new UserContextDto(26, null, "Högstadiet", "Become a doctor");
        var expectedOutput = "That's a good plan, for becoming a doctor you should focus on natural science";
        
        _routerMock.Setup(r => r.RouteAndExecuteAsync(userMsg, conversationHistory, context)).ReturnsAsync(Result<string>.Success(expectedOutput));

        // Act
        
        var result = await _service.GetGuidanceAsync(userMsg, conversationHistory, context);

        // Assert

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedOutput, result.Data);
        Assert.Null(result.Error);
        _routerMock.Verify(r => r.RouteAndExecuteAsync(userMsg, conversationHistory, context), Times.Once);
    }
}