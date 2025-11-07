using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketGymnasiumProperties(
    [property: JsonPropertyName("programmes")]
    List<string>? Programmes);