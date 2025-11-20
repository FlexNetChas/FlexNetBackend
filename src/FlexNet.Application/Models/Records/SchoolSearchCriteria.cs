namespace FlexNet.Application.Models.Records;

public record SchoolSearchCriteria(
    string? Municipality ,
    IReadOnlyList<string>? ProgramCodes,
    string? SearchText ,
    int? MaxResult );