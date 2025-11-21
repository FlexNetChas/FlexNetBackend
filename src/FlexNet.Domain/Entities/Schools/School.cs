namespace FlexNet.Domain.Entities.Schools;

public record School(
    string SchoolUnitCode,
    string Name,
    string? Municipality,
    string MunicipalityCode,
    string? WebsiteUrl,
    string? Email,
    string? Phone,
    Address? VisitingAddress,
    Coordinates? Coordinates,
    IReadOnlyList<SchoolProgram> Programs)
    ;