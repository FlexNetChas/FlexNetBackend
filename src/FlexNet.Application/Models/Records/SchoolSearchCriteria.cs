namespace FlexNet.Application.Models.Records;

public record SchoolSearchCriteria(
    string? Municipality = null,
    IReadOnlyList<string>? ProgramCodes = null,
    string? SearchText = null,
    int? MaxResult = 100);