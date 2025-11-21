using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services.Formatters;
using FlexNet.Domain.Entities.Schools;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlexNet.Application.Tests;

public class ProgramCatalogBuilderTests
{
    private readonly Mock<IProgramService> _serviceMock;
    private readonly Mock<ILogger<ProgramCatalogBuilder>> _loggerMock;
    private readonly ProgramCatalogBuilder _builder;

    public ProgramCatalogBuilderTests()
    {
        _serviceMock = new Mock<IProgramService>();
        _loggerMock = new Mock<ILogger<ProgramCatalogBuilder>>();
        _builder = new ProgramCatalogBuilder(_serviceMock.Object, _loggerMock.Object);
    }
    
     [Fact]
    public async Task BuildCatalogXmlAsync_WithPrograms_ReturnsValidXml()
    {
        // Arrange
        var programs = new List<SchoolProgram>
        {
            new(
                Code: "TE25",
                Name: "Teknikprogrammet",
                StudyPaths: new List<StudyPath>
                {
                    new("TEIND", "Industriteknisk inriktning")
                }),
            new(
                Code: "BA25",
                Name: "Bygg- och anläggningsprogrammet",
                StudyPaths: new List<StudyPath>
                {
                    new("BAHUD", "Husbyggnad"),
                    new("BAMAA", "Mark och anläggning")
                })
        };

        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Success(programs));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        var xml = result.Data;
        
        // Verify XML structure
        Assert.Contains("<available_programs>", xml);
        Assert.Contains("</available_programs>", xml);
        
        // Verify programs are included
        Assert.Contains("code=\"TE25\"", xml);
        Assert.Contains("name=\"Teknikprogrammet\"", xml);
        Assert.Contains("code=\"BA25\"", xml);
        Assert.Contains("name=\"Bygg- och anläggningsprogrammet\"", xml);
        
        // Verify study paths are included
        Assert.Contains("code=\"TEIND\"", xml);
        Assert.Contains("name=\"Industriteknisk inriktning\"", xml);
        Assert.Contains("code=\"BAHUD\"", xml);
        Assert.Contains("code=\"BAMAA\"", xml);
        
        // Verify XML tags structure
        Assert.Contains("<study_paths>", xml);
        Assert.Contains("</study_paths>", xml);
        Assert.Contains("<path", xml);
    }

    [Fact]
    public async Task BuildCatalogXmlAsync_SortsProgramsByCode()
    {
        // Arrange - Programs in reverse order
        var programs = new List<SchoolProgram>
        {
            new("TE25", "Teknikprogrammet", new List<StudyPath>()),
            new("NA25", "Naturvetenskapsprogrammet", new List<StudyPath>()),
            new("BA25", "Bygg- och anläggningsprogrammet", new List<StudyPath>())
        };

        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Success(programs));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var xml = result.Data!;
        
        // Verify BA25 appears before NA25, which appears before TE25
        var ba25Index = xml.IndexOf("code=\"BA25\"", StringComparison.Ordinal);
        var na25Index = xml.IndexOf("code=\"NA25\"", StringComparison.Ordinal);
        var te25Index = xml.IndexOf("code=\"TE25\"", StringComparison.Ordinal);
        
        Assert.True(ba25Index < na25Index, "BA25 should appear before NA25");
        Assert.True(na25Index < te25Index, "NA25 should appear before TE25");
    }

    [Fact]
    public async Task BuildCatalogXmlAsync_WithProgramsWithoutStudyPaths_BuildsValidXml()
    {
        // Arrange
        var programs = new List<SchoolProgram>
        {
            new(
                Code: "EK25",
                Name: "Ekonomiprogrammet",
                StudyPaths: new List<StudyPath>())  // Empty study paths
        };

        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Success(programs));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var xml = result.Data!;
        
        Assert.Contains("code=\"EK25\"", xml);
        Assert.Contains("name=\"Ekonomiprogrammet\"", xml);
        
        // Should not have study_paths section if empty
        Assert.DoesNotContain("<study_paths>", xml);
    }

    [Fact]
    public async Task BuildCatalogXmlAsync_WhenProgramServiceFails_ReturnsFailure()
    {
        // Arrange
        var error = new ErrorInfo(
            ErrorCode: "SERVICE_ERROR",
            CanRetry: true,
            RetryAfter: 60,
            Message: "Program service failed");

        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Failure(error));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("SERVICE_ERROR", result.Error.ErrorCode);
    }

    [Fact]
    public async Task BuildCatalogXmlAsync_WhenNoProgramsAvailable_ReturnsFailure()
    {
        // Arrange
        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Success(new List<SchoolProgram>()));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("NO_PROGRAMS", result.Error.ErrorCode);
    }

    [Fact]
    public async Task BuildCatalogXmlAsync_EscapesXmlSpecialCharacters()
    {
        // Arrange - Program with special characters
        var programs = new List<SchoolProgram>
        {
            new(
                Code: "TEST",
                Name: "Program with <special> & \"characters\"",
                StudyPaths: new List<StudyPath>
                {
                    new("PATH1", "Study path with 'quotes' & <tags>")
                })
        };

        _serviceMock
            .Setup(x => x.GetAllProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<SchoolProgram>>.Success(programs));

        // Act
        var result = await _builder.BuildCatalogXmlAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var xml = result.Data!;
        
        // Verify special characters are escaped
        Assert.Contains("&lt;special&gt;", xml);
        Assert.Contains("&amp;", xml);
        Assert.Contains("&quot;", xml);
        Assert.Contains("&apos;", xml);
        
        // Verify unescaped characters are NOT present
        Assert.DoesNotContain("with <special>", xml);
        Assert.DoesNotContain("with 'quotes'", xml);
    }
}