using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketListResponse(
    [property: JsonPropertyName("data")]
    SkolverketListData Data);
    
    public record SkolverketListData(
        [property: JsonPropertyName("type")]
        string? Type,
        
        [property: JsonPropertyName("attributes")]
        List<SkolverketSchoolSummary> Attributes);
        
        public record SkolverketSchoolSummary(
            [property: JsonPropertyName("schoolUnitCode")]
            string SchoolUnitCode,
            [property: JsonPropertyName("name")]
            string Name,
            [property: JsonPropertyName("status")]
            string Status);