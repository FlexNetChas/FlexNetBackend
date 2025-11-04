using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketGeoCoordinates(
    [property: JsonPropertyName("latitude")]
    string? Latitude,
    [property: JsonPropertyName("longitude")]
    string? Longitude);