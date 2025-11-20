using System.Security;
using System.Text;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Services;

/// <summary>
/// Enriches AI prompts with additional context (schools, search results, etc.)
/// for the AI to read and understand.
/// Note: This is different from SchoolResponseFormatter which formats output for users.
/// </summary>
public class PromptEnricher : IPromptEnricher
{
    /// <summary>
    /// Enriches prompt with school search results for AI to provide personalized advice.
    /// </summary>
    public string EnrichWithSchools(string xmlPrompt, List<School> schools)
    {
        if (schools == null || schools.Count == 0)
            return xmlPrompt;
        
        var sb = new StringBuilder();
        sb.AppendLine(xmlPrompt);
        sb.AppendLine();
        
        // Add school search results section
        sb.AppendLine("<school_search_results>");
        sb.AppendLine($"  <result_count>{schools.Count}</result_count>");
        sb.AppendLine("  <schools>");
        
        foreach (var school in schools.Take(5))  // Top 5 schools
        {
            sb.AppendLine("    <school>");
            sb.AppendLine($"      <name>{EscapeXml(school.Name)}</name>");
            sb.AppendLine($"      <municipality>{EscapeXml(school.Municipality)}</municipality>");
            
            // Programs
            if (school.Programs.Any())
            {
                sb.AppendLine("      <programs>");
                foreach (var program in school.Programs.Take(3))
                {
                    sb.AppendLine($"        <program>{EscapeXml(program.Name)}</program>");
                }
                sb.AppendLine("      </programs>");
            }
            
            // Contact info
            if (!string.IsNullOrEmpty(school.WebsiteUrl))
                sb.AppendLine($"      <website>{EscapeXml(school.WebsiteUrl)}</website>");
            
            if (!string.IsNullOrEmpty(school.Phone))
                sb.AppendLine($"      <phone>{EscapeXml(school.Phone)}</phone>");
            
            if (!string.IsNullOrEmpty(school.Email))
                sb.AppendLine($"      <email>{EscapeXml(school.Email)}</email>");
            
            sb.AppendLine("    </school>");
        }
        
        sb.AppendLine("  </schools>");
        sb.AppendLine("</school_search_results>");
        sb.AppendLine();
        
        // Instructions for AI
        sb.AppendLine("<guidance_instructions>");
        sb.AppendLine("Skriv 3-5 meningar på svenska som:");
        sb.AppendLine("1. Bekräftar elevens intresse");
        sb.AppendLine("2. Refererar till SPECIFIKA skolor och deras program");
        sb.AppendLine("3. Uppmuntrar att besöka webbsidor (nämn specifika URL:er)");
        sb.AppendLine("4. Föreslår att kontakta skolor direkt (nämn telefonnummer eller e-post)");
        sb.AppendLine("5. Erbjuder hjälp med fler frågor");
        sb.AppendLine();
        sb.AppendLine("Var varm, personlig och specifik. Använd informationen om skolorna ovan.");
        sb.AppendLine("Skriv ENDAST rådgivningstexten (skolistan visas separat).");
        sb.AppendLine("</guidance_instructions>");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Enriches prompt when no schools were found in search.
    /// </summary>
    public string EnrichWithNoResults(string xmlPrompt, SchoolRequestInfo searchCriteria)
    {
        var sb = new StringBuilder();
        sb.AppendLine(xmlPrompt);
        sb.AppendLine();
        
        sb.AppendLine("<search_results>");
        sb.AppendLine("  <status>no_results_found</status>");
        sb.AppendLine("  <search_criteria>");
        
        if (!string.IsNullOrEmpty(searchCriteria.Municipality))
            sb.AppendLine($"    <municipality>{EscapeXml(searchCriteria.Municipality)}</municipality>");
        
        if (searchCriteria.ProgramCodes?.Any() == true)
        {
            sb.AppendLine("    <programs>");
            foreach (var code in searchCriteria.ProgramCodes)
            {
                sb.AppendLine($"      <program_code>{EscapeXml(code)}</program_code>");
            }
            sb.AppendLine("    </programs>");
        }
        
        sb.AppendLine("  </search_criteria>");
        sb.AppendLine("</search_results>");
        sb.AppendLine();
        
        sb.AppendLine("<guidance_instructions>");
        sb.AppendLine("Hjälp eleven på svenska genom att:");
        sb.AppendLine("1. Föreslå närliggande kommuner");
        sb.AppendLine("2. Fråga om de kan överväga relaterade program");
        sb.AppendLine("3. Uppmuntra dem att specificera sina sökkriterier");
        sb.AppendLine();
        sb.AppendLine("Var stödjande och konstruktiv.");
        sb.AppendLine("</guidance_instructions>");
        
        return sb.ToString();
    }
    
    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        
        return SecurityElement.Escape(value);
    }
}