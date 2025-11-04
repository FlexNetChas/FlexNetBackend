using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketAddress(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("streetAddress")]
    string? StreetAddress,
    [property: JsonPropertyName("postalCode")]
    string? PostalCode,
    [property: JsonPropertyName("locality")]
    string? Locality,
    [property: JsonPropertyName("geoCoordinates")]
    SkolverketGeoCoordinates? GeoCoordinates);
    