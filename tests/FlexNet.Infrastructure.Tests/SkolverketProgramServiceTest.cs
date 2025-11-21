using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Services.Skolverket;
using FlexNet.Infrastructure.Services.Skolverket.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlexNet.Infrastructure.Tests;

public class SkolverketProgramServiceTest
{
    private readonly Mock<ISkolverketApiClient> _clientMock;
    private readonly Mock<ILogger<SkolverketProgramService>> _loggerMock;
    private readonly IMemoryCache _cache;
    private readonly SkolverketProgramService _service;
    
    public SkolverketProgramServiceTest()
    {
        _clientMock = new Mock<ISkolverketApiClient>();
        _loggerMock = new Mock<ILogger<SkolverketProgramService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        
        _service = new SkolverketProgramService(
            _clientMock.Object,
            _loggerMock.Object,
            _cache);
    }
    
    [Fact]
    public async Task GetAllProgramsAsync_WhenCalled_ReturnsPrograms()
    {
        // Arrange
        var mockResponse = new SkolverketProgramsResponse(
            Status: "OK",
            Message: "Success",
            Body: new ProgramsBody(
                Gr: new List<ProgramDto>(),
                Gran: new List<ProgramDto>(),
                Gy: new List<ProgramDto>
                {
                    new ProgramDto(
                        Code: "TE25",
                        Name: "Teknikprogrammet",
                        StudyPaths: new List<StudyPathDto>
                        {
                            new(Code: "TEIND", Name: "Industriteknisk inriktning")
                        })
                }));

        _clientMock
            .Setup(x => x.GetProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetAllProgramsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        
        var program = result.Data.First();
        Assert.NotNull(program);
        Assert.Equal("TE25", program.Code);
        Assert.Equal("Teknikprogrammet", program.Name);
        Assert.NotNull(program.StudyPaths);
        Assert.Equal("TEIND", program.StudyPaths[0].Code);
    }

    [Fact]
    public async Task GetAllProgramsAsync_UsesCaching_OnSecondCall()
    {
        // Arrange
        var mockResponse = new SkolverketProgramsResponse(
            Status: "OK",
            Message: "Success",
            Body: new ProgramsBody(
                Gr: new List<ProgramDto>(),
                Gran: new List<ProgramDto>(),
                Gy: new List<ProgramDto>
                {
                    new(Code: "BA25", Name: "Bygg- och anläggningsprogrammet", StudyPaths: new List<StudyPathDto>())
                }));

        _clientMock
            .Setup(x => x.GetProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act - First call
        await _service.GetAllProgramsAsync();
        
        // Act - Second call (should use cache)
        var result = await _service.GetAllProgramsAsync();

        // Assert
        _clientMock.Verify(
            x => x.GetProgramsAsync(It.IsAny<CancellationToken>()), 
            Times.Once); // API called only once, second used cache
        
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetProgramByCodeAsync_WhenProgramExists_ReturnsProgram()
    {
        // Arrange
        var mockResponse = new SkolverketProgramsResponse(
            Status: "OK",
            Message: "Success",
            Body: new ProgramsBody(
                Gr: new List<ProgramDto>(),
                Gran: new List<ProgramDto>(),
                Gy: new List<ProgramDto>
                {
                    new (Code: "NA25", Name: "Naturvetenskapsprogrammet", StudyPaths: new List<StudyPathDto>())
                }));

        _clientMock
            .Setup(x => x.GetProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetProgramByCodeAsync("NA25");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("NA25", result.Data.Code);
        Assert.Equal("Naturvetenskapsprogrammet", result.Data.Name);
    }

    [Fact]
    public async Task GetProgramByCodeAsync_WhenProgramNotFound_ReturnsFailure()
    {
        // Arrange
        var mockResponse = new SkolverketProgramsResponse(
            Status: "OK",
            Message: "Success",
            Body: new ProgramsBody(
                Gr: new List<ProgramDto>(),
                Gran: new List<ProgramDto>(),
                Gy: new List<ProgramDto>()));

        _clientMock
            .Setup(x => x.GetProgramsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetProgramByCodeAsync("INVALID");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("PROGRAM_NOT_FOUND", result.Error?.ErrorCode);
    }
}