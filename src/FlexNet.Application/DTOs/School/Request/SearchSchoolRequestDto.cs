namespace FlexNet.Application.DTOs.School.Request;
    public record SearchSchoolsRequestDto(
        string? Municipality = null,
        List<string>? ProgramCodes = null,
        string? SearchText = null,
        int? MaxResults = 100
    ); 
