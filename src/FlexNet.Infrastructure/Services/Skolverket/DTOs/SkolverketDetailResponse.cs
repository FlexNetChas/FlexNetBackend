using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketDetailResponse(
    [property: JsonPropertyName("data")]
    SkolverketSchoolData Data);