using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;
using FlexNet.Infrastructure.Services.Skolverket.DTOs;
using Microsoft.Extensions.Logging;
using static System.Double;
using System.Globalization;
namespace FlexNet.Infrastructure.Services.Skolverket;

public class SkolverketMapper
{
    private readonly ILogger<SkolverketMapper> _logger;
    private static readonly Dictionary<string, string> ProgramNames = new()
    {
        ["BA"] = "Barn- och fritidsprogrammet",
        ["BF"] = "Bygg- och anläggningsprogrammet",
        ["EE"] = "El- och energiprogrammet",
        ["EK"] = "Ekonomiprogrammet",
        ["ES"] = "Estetiska programmet",
        ["FT"] = "Fordonsprogrammet",
        ["HA"] = "Hantverksprogrammet",
        ["HT"] = "Handels- och administrationsprogrammet",
        ["HU"] = "Humanistiska programmet",
        ["IN"] = "Industritekniska programmet",
        ["NA"] = "Naturvetenskapsprogrammet",
        ["RL"] = "Restaurang- och livsmedelsprogrammet",
        ["SA"] = "Samhällsvetenskapsprogrammet",
        ["TE"] = "Teknikprogrammet",
        ["VF"] = "Vård- och omsorgsprogrammet",
        
        // 2025 codes (with "25" suffix - same names)
        ["BA25"] = "Barn- och fritidsprogrammet",
        ["BF25"] = "Bygg- och anläggningsprogrammet",
        ["EE25"] = "El- och energiprogrammet",
        ["EK25"] = "Ekonomiprogrammet",
        ["ES25"] = "Estetiska programmet",
        ["FT25"] = "Fordonsprogrammet",
        ["HA25"] = "Hantverksprogrammet",
        ["HT25"] = "Handels- och administrationsprogrammet",
        ["HU25"] = "Humanistiska programmet",
        ["IN25"] = "Industritekniska programmet",
        ["NA25"] = "Naturvetenskapsprogrammet",
        ["RL25"] = "Restaurang- och livsmedelsprogrammet",
        ["SA25"] = "Samhällsvetenskapsprogrammet",
        ["TE25"] = "Teknikprogrammet",
        ["VF25"] = "Vård- och omsorgsprogrammet"
    };

    public School? ToSchool(SkolverketSchoolData? data)
    {
        if (data == null)
        {
            _logger.LogWarning("Received null school data");
            return null;
        }

        var attributes = data.Attributes;

        if (string.IsNullOrWhiteSpace(data.SchoolUnitCode))
        {
            _logger.LogWarning("School missing SchoolUnitCode");
            return null;
        }

        if (string.IsNullOrWhiteSpace(attributes.DisplayName))
        {
            _logger.LogWarning("School {Code} missing DisplayName", data.SchoolUnitCode);
            return null;
        }

        var visitingAddress = ExtractVisitingAddress(attributes.Addresses);
        var coordinates = visitingAddress != null ? ParseCoordinates(attributes.Addresses?
            .FirstOrDefault(a => a.Type == "BESOKSADRESS")?.GeoCoordinates) : null;
        
        var municipalityName = GetMunicipalityName(attributes.Addresses, attributes.MunicipalityCode);
        var programs = MapPrograms(attributes.SchoolTypeProperties);
        
        return new School(
            SchoolUnitCode: data.SchoolUnitCode,
            Name: attributes.DisplayName,
            Municipality: municipalityName,
            MunicipalityCode: attributes.MunicipalityCode ?? "Unknown",
            WebsiteUrl: attributes.Url,
            Email: attributes.Email,
            Phone:  attributes.PhoneNumber,
            VisitingAddress: visitingAddress,
            Coordinates: coordinates,
            Programs: programs);
    }
    private IReadOnlyList<SchoolProgram> MapPrograms(SkolverketSchoolTypeProperties? properties)
    {
        if (properties?.Gy?.Programmes == null || properties.Gy.Programmes.Count == 0)
        {
            _logger.LogWarning("School has no programs listed");
            return Array.Empty<SchoolProgram>();
        }
        var programs = properties.Gy.Programmes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => CreateProgram(code))
            .ToList();
        return programs;
        ;
    }

    private SchoolProgram CreateProgram(string code)
    {
        var name = GetProgramName(code);
        return new SchoolProgram(
            Code: code,
            Name: name,
            Orientation: null);
    }

    private string GetProgramName(string code)
    {
        if (ProgramNames.TryGetValue(code, out var name))
            return name;

        if (code.Length > 2)
        {
            var normalized = code.Substring(0, 2);
            if(ProgramNames.TryGetValue(normalized, out var normalizedName))
                return normalizedName;
        }
        _logger.LogWarning("Unknown program code: {Code}", code);
        return $"Program {code}";
    }

    public SkolverketMapper(ILogger<SkolverketMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    private Coordinates? ParseCoordinates(SkolverketGeoCoordinates? geoCoordinates)
    {
        if (geoCoordinates == null) 
            return null;
    
        if (string.IsNullOrWhiteSpace(geoCoordinates.Latitude) || 
            string.IsNullOrWhiteSpace(geoCoordinates.Longitude))
            return null;
    
        var latSuccess = double.TryParse(geoCoordinates.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude);
        var lonSuccess = double.TryParse(geoCoordinates.Longitude,NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude);
    
        if (!latSuccess || !lonSuccess)
        {
           
            _logger.LogWarning(
                "Failed to parse coordinates: lat={Latitude}, lon={Longitude}",
                geoCoordinates.Latitude, 
                geoCoordinates.Longitude);
        
            return null;
        }
    
        return new Coordinates(latitude, longitude);  
        
    }

    private Address? ExtractVisitingAddress(List<SkolverketAddress>? addresses)
    {
        if(addresses == null || addresses.Count == 0) return null;
        var address = addresses.FirstOrDefault(a => a.Type == "BESOKSADRESS") ??
                      addresses.FirstOrDefault(a => a.Type == "POSTADRESS");
        if (address == null) return null;

        if (string.IsNullOrWhiteSpace(address.StreetAddress) ||
            string.IsNullOrWhiteSpace(address.PostalCode) ||
            string.IsNullOrWhiteSpace(address.Locality))
        {
            _logger.LogWarning("Address missing required fields for school");
            return null;
        }
        return new Address(address.StreetAddress, address.PostalCode, address.Locality);
    }

    private string? GetMunicipalityName(List<SkolverketAddress>? addresses, string municipalityCode)
    {
        if(addresses == null || addresses.Count == 0) return municipalityCode ;

        var address = addresses.FirstOrDefault(a => a.Type == "BESOKSADRESS") ??
                      addresses.FirstOrDefault(a => a.Type == "POSTADRESS");

        if (address == null || string.IsNullOrWhiteSpace(address.Locality)) return municipalityCode;
        return address.Locality;
    }

}