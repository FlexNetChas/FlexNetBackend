using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketSchoolData(
    [property: JsonPropertyName("type")] 
    string? Type,  
    
    [property: JsonPropertyName("schoolUnitCode")]
    string SchoolUnitCode,
    
    [property: JsonPropertyName("attributes")]
   SkolverketSchoolAttributes Attributes
);