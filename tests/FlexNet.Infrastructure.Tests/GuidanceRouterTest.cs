using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
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
    private readonly Mock<ISchoolSearchDetector> _detectorMock;
    private readonly Mock<IPromptEnricher> _enricherMock;
    private readonly Mock<IAiClient> _aiClientMock;
    private readonly GuidanceRouter _router;
    private readonly ITestOutputHelper _output;

    public GuidanceRouterTest(ITestOutputHelper output)
    {
        _schoolServiceMock = new Mock<ISchoolService>();
        _loggerMock = new Mock<ILogger<GuidanceRouter>>();
        _detectorMock = new Mock<ISchoolSearchDetector>();
        _enricherMock = new Mock<IPromptEnricher>();
        _aiClientMock = new Mock<IAiClient>();
        
        _router = new GuidanceRouter(
            _schoolServiceMock.Object,
            _loggerMock.Object,
            _detectorMock.Object,
            _enricherMock.Object,
            _aiClientMock.Object
        );

        _output = output;
    }

    [Fact]
    public async Task Should_UseRegularGenerator_WhenNoSchoolSearchDetected()
    {
        // Arrange

        const string xmlPrompt = "<current_message>Hello, I need general advice</current_message>";
        var conversationHistory = Enumerable.Empty<ConversationMessage>();
        var context = new UserContextDto(26, null, "Högstadiet", "Become a doctor");
        const string expectedResponse = "Let me help ypu explore your options...";

        // Mock: Detector returns null (no school search)
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns((SchoolRequestInfo?)null);

        // Mock: AI client returns response
        _aiClientMock
            .Setup(a => a.CallAsync(xmlPrompt))
            .ReturnsAsync(Result<string>.Success(expectedResponse));
        
        // Act
        
        var result = await _router.RouteAndExecuteAsync(xmlPrompt, conversationHistory, context);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse, result.Data);
        
        // Verify AI client was called with original prompt (not enriched)
        _aiClientMock.Verify(a => a.CallAsync(xmlPrompt), Times.Once);
        
        // Verify enricher was NOT called (regular counseling doesn't need enrichment)
        _enricherMock.Verify(e => e.EnrichWithSchools(It.IsAny<string>(), It.IsAny<List<School>>()), Times.Never);
        _enricherMock.Verify(e => e.EnrichWithNoResults(It.IsAny<string>(), It.IsAny<SchoolRequestInfo>()), Times.Never);
    }
[Fact]
    public async Task RouteAndExecute_Should_EnrichAndCallAi_WhenSchoolsFound()
    {
        // Arrange
        const string xmlPrompt = "<current_message>Jag vill studera teknik i Stockholm</current_message>";
        var conversationHistory = new List<ConversationMessage>();
        if (conversationHistory == null) throw new ArgumentNullException(nameof(conversationHistory));
        var context = new UserContextDto(18, null, "Årskurs 9", "Study technology");
        
        var schoolRequest = new SchoolRequestInfo
        {
            Municipality = "Stockholm",
            ProgramCodes = new List<string> { "TE" }
        };
        
        var schools = new List<School>
        {
            new(
                SchoolUnitCode: "12345678",
                Name: "Kungsholmens Gymnasium",
                Municipality: "Stockholm",
                MunicipalityCode: "0180",
                WebsiteUrl: "https://kungsholmen.stockholm.se",
                Email: "info@kungsholmen.se",
                Phone: "08-508 08 000",
                VisitingAddress: null,
                Coordinates: null,
                Programs: new List<SchoolProgram>
                {
                    new(
                        Code: "TE",
                        Name: "Teknikprogrammet",
                        StudyPaths: new List<StudyPath>()
                    )
                }
            )
        };

        const string enrichedPrompt = xmlPrompt + "\n<school_search_results>...</school_search_results>";
        const string aiAdvice = "Kungsholmens Gymnasium är ett utmärkt val...";

        // Mock: Detector detects school search
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns(schoolRequest);

        // Mock: School service returns schools
        _schoolServiceMock
            .Setup(s => s.SearchSchoolsAsync(
                It.IsNotNull<SchoolSearchCriteria>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<School>>.Success(schools));

        // Mock: Enricher enriches prompt
        _enricherMock
            .Setup(e => e.EnrichWithSchools(xmlPrompt, schools))
            .Returns(enrichedPrompt);

        // Mock: AI client responds to enriched prompt
        _aiClientMock
            .Setup(a => a.CallAsync(enrichedPrompt))
            .ReturnsAsync(Result<string>.Success(aiAdvice));

        // Act
        var result = await _router.RouteAndExecuteAsync(xmlPrompt, conversationHistory, context);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        
        // Verify enricher was called with schools
        _enricherMock.Verify(e => e.EnrichWithSchools(xmlPrompt, schools), Times.Once);
        
        // Verify AI client was called with ENRICHED prompt
        _aiClientMock.Verify(a => a.CallAsync(enrichedPrompt), Times.Once);
        
        // Verify school service was called
        _schoolServiceMock.Verify(
            s => s.SearchSchoolsAsync(
                It.IsNotNull<SchoolSearchCriteria>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task RouteAndExecute_Should_EnrichWithNoResults_WhenNoSchoolsFound()
    {
        // Arrange
        const string xmlPrompt = "<current_message>Jag vill studera rymdteknik i Pajala</current_message>";
        var conversationHistory = new List<ConversationMessage>();
        var context = new UserContextDto(18, null, "Årskurs 9", "Study space technology");
        
        var schoolRequest = new SchoolRequestInfo
        {
            Municipality = "Pajala",
            ProgramCodes = ["TE"]
        };
        
        const string noResultsPrompt = xmlPrompt + "\n<search_results><status>no_results_found</status></search_results>";
        const string aiResponse = "Tyvärr hittade jag inga skolor i Pajala...";

        // Mock: Detector detects school search
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns(schoolRequest);

        // Mock: School service returns empty list
        _schoolServiceMock
            .Setup(s => s.SearchSchoolsAsync(
                It.IsNotNull<SchoolSearchCriteria>(), 
                It.IsAny<CancellationToken>()))            .ReturnsAsync(Result<IEnumerable<School>>.Success(new List<School>()));

        // Mock: Enricher creates no-results prompt
        _enricherMock
            .Setup(e => e.EnrichWithNoResults(xmlPrompt, schoolRequest))
            .Returns(noResultsPrompt);

        // Mock: AI responds to no-results prompt
        _aiClientMock
            .Setup(a => a.CallAsync(noResultsPrompt))
            .ReturnsAsync(Result<string>.Success(aiResponse));

        // Act
        var result = await _router.RouteAndExecuteAsync(xmlPrompt, conversationHistory, context);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(aiResponse, result.Data);
        
        // Verify no-results enrichment was used
        _enricherMock.Verify(e => e.EnrichWithNoResults(xmlPrompt, schoolRequest), Times.Once);
        
        // Verify AI was called with no-results prompt
        _aiClientMock.Verify(a => a.CallAsync(noResultsPrompt), Times.Once);
        
        // Verify school enrichment was NOT used
        _enricherMock.Verify(e => e.EnrichWithSchools(It.IsAny<string>(), It.IsAny<List<School>>()), Times.Never);
    }
    [Fact]
    public async Task RouteAndExecuteStreaming_Should_StreamDirectly_WhenNoSchoolSearch()
    {
        // Arrange
        const string xmlPrompt = "<current_message>Hello</current_message>";
        var conversationHistory = new List<ConversationMessage>();
        var context = new UserContextDto(26, null, "Högstadiet", "General advice");
        var aiChunks = new[] { "Hello, ", "how ", "can ", "I ", "help?" };

        // Mock: Detector returns null
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns((SchoolRequestInfo?)null);

        // Mock: AI client streams chunks
        _aiClientMock
            .Setup(a => a.CallStreamingAsync(xmlPrompt))
            .Returns(CreateAsyncEnumerable(aiChunks));

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in _router.RouteAndExecuteStreamingAsync(xmlPrompt, conversationHistory, context))
        {
            Assert.True(chunk.IsSuccess);
            chunks.Add(chunk.Data!);
        }

        // Assert
        Assert.Equal(aiChunks.Length, chunks.Count);
        Assert.Equal(string.Join("", aiChunks), string.Join("", chunks));
        
        // Verify streaming was called with original prompt
        _aiClientMock.Verify(a => a.CallStreamingAsync(xmlPrompt), Times.Once);
        
        // Verify no enrichment
        _enricherMock.Verify(e => e.EnrichWithSchools(It.IsAny<string>(), It.IsAny<List<School>>()), Times.Never);
    }

    [Fact]
    public async Task RouteAndExecuteStreaming_Should_StreamWithSchoolList_WhenSchoolsFound()
    {
        // Arrange
        const string xmlPrompt = "<current_message>Jag vill studera teknik</current_message>";
        var conversationHistory = new List<ConversationMessage>();
        var context = new UserContextDto(18, null, "Årskurs 9", "Technology");
        
        var schoolRequest = new SchoolRequestInfo
        {
            Municipality = "Stockholm",
            ProgramCodes = new List<string> { "TE" }
        };
        
        var schools = new List<School>
        {
            new(
                SchoolUnitCode: "87654321",
                Name: "Test Gymnasium",
                Municipality: "Stockholm",
                MunicipalityCode: "0180",
                WebsiteUrl: null,
                Email: null,
                Phone: null,
                VisitingAddress: null,
                Coordinates: null,
                Programs: new List<SchoolProgram>
                {
                    new("TE", "Teknik", null)
                }
            )
        };

        const string enrichedPrompt = xmlPrompt + "\n<schools>...</schools>";
        var aiChunks = new[] { "Great ", "schools ", "for ", "you!" };

        // Mock: Detector detects school search
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns(schoolRequest);

        // Mock: School service returns schools
        _schoolServiceMock
            .Setup(s => s.SearchSchoolsAsync(
                It.IsNotNull<SchoolSearchCriteria>(), 
                It.IsAny<CancellationToken>()))            .ReturnsAsync(Result<IEnumerable<School>>.Success(schools));

        // Mock: Enricher
        _enricherMock
            .Setup(e => e.EnrichWithSchools(xmlPrompt, schools))
            .Returns(enrichedPrompt);

        // Mock: AI streams
        _aiClientMock
            .Setup(a => a.CallStreamingAsync(enrichedPrompt))
            .Returns(CreateAsyncEnumerable(aiChunks));

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in _router.RouteAndExecuteStreamingAsync(xmlPrompt, conversationHistory, context))
        {
            Assert.True(chunk.IsSuccess);
            chunks.Add(chunk.Data!);
        }

        // Assert
        // Should have AI chunks + school list chunk
        Assert.True(chunks.Count > aiChunks.Length); // School list appended
        
        // Verify last chunk contains school formatting
        var lastChunk = chunks.Last();
        Assert.Contains("Schools from Skolverkets official register", lastChunk);
        
        // Verify enrichment was used
        _enricherMock.Verify(e => e.EnrichWithSchools(xmlPrompt, schools), Times.Once);
        _aiClientMock.Verify(a => a.CallStreamingAsync(enrichedPrompt), Times.Once);
    }

    [Fact]
    public async Task RouteAndExecuteStreaming_Should_HandleNoResults_WhenSchoolSearchReturnsEmpty()
    {
        // Arrange
        const string xmlPrompt = "<current_message>Rymdteknik i Pajala</current_message>";
        var conversationHistory = new List<ConversationMessage>();
        var context = new UserContextDto(18, null, "Årskurs 9", "Space tech");
        
        var schoolRequest = new SchoolRequestInfo
        {
            Municipality = "Pajala",
            ProgramCodes = new List<string> { "TE" }
        };
        
        const string noResultsPrompt = xmlPrompt + "\n<no_results>...</no_results>";
        var aiChunks = new[] { "Tyvärr ", "inga ", "resultat..." };

        // Mock: Detector detects search
        _detectorMock
            .Setup(d => d.DetectSchoolRequest(It.IsAny<string>(), conversationHistory))
            .Returns(schoolRequest);

        // Mock: No schools found
        _schoolServiceMock
            .Setup(s => s.SearchSchoolsAsync(
                It.IsNotNull<SchoolSearchCriteria>(), 
                It.IsAny<CancellationToken>()))            .ReturnsAsync(Result<IEnumerable<School>>.Success(new List<School>()));

        // Mock: No-results enrichment
        _enricherMock
            .Setup(e => e.EnrichWithNoResults(xmlPrompt, schoolRequest))
            .Returns(noResultsPrompt);

        // Mock: AI streams no-results advice
        _aiClientMock
            .Setup(a => a.CallStreamingAsync(noResultsPrompt))
            .Returns(CreateAsyncEnumerable(aiChunks));

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in _router.RouteAndExecuteStreamingAsync(xmlPrompt, conversationHistory, context))
        {
            Assert.True(chunk.IsSuccess);
            chunks.Add(chunk.Data!);
        }

        // Assert
        Assert.Equal(aiChunks.Length, chunks.Count);
        
        // Verify no-results enrichment
        _enricherMock.Verify(e => e.EnrichWithNoResults(xmlPrompt, schoolRequest), Times.Once);
        _aiClientMock.Verify(a => a.CallStreamingAsync(noResultsPrompt), Times.Once);
        
        // Verify school enrichment NOT used
        _enricherMock.Verify(e => e.EnrichWithSchools(It.IsAny<string>(), It.IsAny<List<School>>()), Times.Never);
    }



    /// <summary>
    /// Creates an async enumerable from an array of strings for mocking streaming responses.
    /// </summary>
    private static async IAsyncEnumerable<Result<string>> CreateAsyncEnumerable(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return Result<string>.Success(chunk);
            await Task.Delay(10); // Small delay to simulate streaming
        }
    }

}