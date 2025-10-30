using System.Text.Json.Serialization;

namespace FlexNet.Infrastructure.Services.Skolverket.DTOs;

public record SkolverketSchoolAttributes(
    [property: JsonPropertyName("displayName")]
    string DisplayName,
    
    [property: JsonPropertyName("status")]
    string Status,  
    
    [property: JsonPropertyName("municipalityCode")]
    string MunicipalityCode,  
    
    [property: JsonPropertyName("url")]
    string? Url,
    
    [property: JsonPropertyName("email")]
    string? Email,
    
    [property: JsonPropertyName("phoneNumber")]
    string? PhoneNumber,
    
   
    [property: JsonPropertyName("addresses")]
    List<SkolverketAddress>? Addresses,
    
    [property: JsonPropertyName("schoolTypeProperties")]
    SkolverketSchoolTypeProperties? SchoolTypeProperties
);