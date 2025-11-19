namespace FlexNet.Application.DTOs.Export;

/// <summary>
/// Metadata about the data export operation
/// </summary>
public record ExportMetadataDto
(
    string Platform,
    string Version, 
    string Reason 
);