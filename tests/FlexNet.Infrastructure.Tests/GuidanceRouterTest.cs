using FlexNet.Application.Configuration;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services;
using FlexNet.Application.Services.AiGenerators;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Services.Gemini;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace FlexNet.Infrastructure.Tests;

public class GuidanceRouterTest
{
    private readonly Mock<ISchoolService> _schoolServiceMock;
    private readonly Mock<ILogger<GuidanceRouter>> _loggerMock;
    private readonly Mock<ISchoolAdviceGenerator> _adviceGeneratorMock;
    private readonly Mock<ISchoolSearchDetector> _detectorMock;
    private readonly Mock<INoResultsGenerator> _noResultsGeneratorMock;
    private readonly Mock<IRegularCounselingGenerator> _regularGeneratorMock;
    private readonly GuidanceRouter _router;
    private readonly ITestOutputHelper _output;

    public GuidanceRouterTest(ITestOutputHelper output)
    {
        _schoolServiceMock = new Mock<ISchoolService>();
        _loggerMock = new Mock<ILogger<GuidanceRouter>>();
        _detectorMock = new Mock<ISchoolSearchDetector>();
        _adviceGeneratorMock = new Mock<ISchoolAdviceGenerator>();
        _noResultsGeneratorMock = new Mock<INoResultsGenerator>();
        _regularGeneratorMock = new Mock<IRegularCounselingGenerator>();
        
        _router = new GuidanceRouter(
            _schoolServiceMock.Object,
            _loggerMock.Object,
            _detectorMock.Object,
            _adviceGeneratorMock.Object,
            _noResultsGeneratorMock.Object,
            _regularGeneratorMock.Object
        );

        _output = output;
    }

    [Fact]
    public async Task Should_UseRegularGenerator_WhenNoSchoolSearchDetected()
    {
        // Arrange

        var userMsg = "Hello, I don't know what to study.";
        var conversationHistory = Enumerable.Empty<ConversationMessage>();
        var context = new UserContextDto(26, null, "Högstadiet", "Become a doctor");
        var expectedResponse = "Let me help ypu explore your options...";

        _regularGeneratorMock.Setup(g => g.GenerateAsync(userMsg, conversationHistory, context))
            .ReturnsAsync(Result<string>.Success(expectedResponse));
        // Act
        
        var result = await _router.RouteAndExecuteAsync(userMsg, conversationHistory, context);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        _regularGeneratorMock.Verify(g => g.GenerateAsync(userMsg, conversationHistory, context), Times.Once);
        _adviceGeneratorMock.Verify(g => g.GenerateAdviceAsync(It.IsAny<string>(), It.IsAny<List<School>>(), It.IsAny<UserContextDto>()), Times.Never); 
        _noResultsGeneratorMock.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<SchoolRequestInfo>(),It.IsAny<UserContextDto>()), Times.Never);
    }
}