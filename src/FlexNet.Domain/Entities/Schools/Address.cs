namespace FlexNet.Domain.Entities.Schools;

public record Address(
    string? StreetAddress,
    string? PostalCode,
    string? Locality);