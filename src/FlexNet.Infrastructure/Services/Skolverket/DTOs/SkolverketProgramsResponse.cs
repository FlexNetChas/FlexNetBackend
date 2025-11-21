using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;


    /// <summary>
    /// Response from Skolverket's /support/programs endpoint
    /// </summary>
    public record SkolverketProgramsResponse(
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("body")] ProgramsBody Body
    );

    public record ProgramsBody(
        [property: JsonPropertyName("gr")] List<ProgramDto> Gr,
        [property: JsonPropertyName("gran")] List<ProgramDto> Gran,
        [property: JsonPropertyName("gy")] List<ProgramDto> Gy
    );

    public record ProgramDto(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("studyPaths")] List<StudyPathDto> StudyPaths
    );

    public record StudyPathDto(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("name")] string Name
    );