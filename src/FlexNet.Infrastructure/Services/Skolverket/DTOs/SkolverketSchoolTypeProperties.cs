using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketSchoolTypeProperties(
    [property: JsonPropertyName("gy")]
    SkolverketGymnasiumProperties? Gy);