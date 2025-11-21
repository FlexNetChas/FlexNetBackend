using System.Text;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.Formatters;

/// <summary>
/// Builds an XML catalog of available gymnasium programs for AI context enrichment.
/// The catalog helps the AI understand which programs exist and match user intent to program codes.
/// </summary>
public class ProgramCatalogBuilder
{
    private readonly IProgramService _programService;
    private readonly ILogger<ProgramCatalogBuilder> _logger;

    public ProgramCatalogBuilder(
        IProgramService programService,
        ILogger<ProgramCatalogBuilder> logger)
    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds XML catalog of all available gymnasium programs.
    /// </summary>
    public async Task<Result<string>> BuildCatalogXmlAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var programsResult = await _programService.GetAllProgramsAsync(cancellationToken);

            if (!programsResult.IsSuccess)
            {
                _logger.LogWarning("Failed to load programs for catalog: {Error}", 
                    programsResult.Error?.Message);
                return Result<string>.Failure(programsResult.Error!);
            }

            if (programsResult.Data == null || !programsResult.Data.Any())
            {
                _logger.LogWarning("No programs available for catalog");
                var error = new ErrorInfo(
                    ErrorCode: "NO_PROGRAMS",
                    CanRetry: true,
                    RetryAfter: 60,
                    Message: "No programs available to build catalog.");
                return Result<string>.Failure(error);
            }

            var programs = programsResult.Data.ToList();
            
            _logger.LogInformation("Building program catalog with {Count} programs", programs.Count);
            
            var xml = BuildXml(programs);
            
            return Result<string>.Success(xml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building program catalog");
            var error = new ErrorInfo(
                ErrorCode: "CATALOG_BUILD_ERROR",
                CanRetry: true,
                RetryAfter: 60,
                Message: "Failed to build program catalog.");
            return Result<string>.Failure(error);
        }
    }

    private static string BuildXml(List<Domain.Entities.Schools.SchoolProgram> programs)
    {
        var xml = new StringBuilder();
        
        xml.AppendLine("<available_programs>");
        
        foreach (var program in programs.OrderBy(p => p.Code))
        {
            xml.AppendLine($"  <program code=\"{EscapeXml(program.Code)}\" name=\"{EscapeXml(program.Name)}\">");
            
            if (program.StudyPaths != null && program.StudyPaths.Any())
            {
                xml.AppendLine("    <study_paths>");
                
                foreach (var path in program.StudyPaths)
                {
                    xml.AppendLine($"      <path code=\"{EscapeXml(path.Code)}\" name=\"{EscapeXml(path.Name)}\"/>");
                }
                
                xml.AppendLine("    </study_paths>");
            }
            
            xml.AppendLine("  </program>");
        }
        
        xml.AppendLine("</available_programs>");
        
        return xml.ToString();
    }

    /// <summary>
    /// Escapes XML special characters to prevent malformed XML.
    /// </summary>
    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}