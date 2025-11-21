using System.Text;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Services.Formatters;

public class SchoolResponseFormatter
{
    public static string FormatSchoolList(string? aiAdvice, List<School> schools)
    {
        
        var response = new StringBuilder();
            
        // Part 1: AI-generated personalized advice
        response.AppendLine("---\n");
        response.AppendLine(aiAdvice);
            
        // Part 2: School list from Skolverket
        response.AppendLine("---\n");
        response.AppendLine("**Schools from Skolverkets official register:**\n");
            
        foreach (var school in schools)
        {
            FormatSchool(response, school);
        }
            
        return response.ToString();
    }
    public static string FormatSchoolListOnly(List<School> schools)
    {
        var response = new StringBuilder();
        
        response.AppendLine("\n\n---\n");
        response.AppendLine("**Schools from Skolverkets official register:**\n");
        
        foreach (var school in schools)
        {
            FormatSchool(response, school);
        }
        
        return response.ToString();
    }
    private static void FormatSchool(StringBuilder response, School school)
    {
        // School name as header
        response.AppendLine($"### {school.Name}");
        response.AppendLine($"📍 **Kommun:** {school.Municipality}");
            
        // Programs offered
        if (school.Programs.Any())
        {
            var programList = string.Join(", ", school.Programs.Take(3).Select(p => p.Name));
            response.AppendLine($"📚 **Program:** {programList}");
        }
            
        // Contact information
        if (!string.IsNullOrEmpty(school.WebsiteUrl))
            response.AppendLine($"🌐 **Webbsida:** {school.WebsiteUrl}");
            
        if (!string.IsNullOrEmpty(school.Phone))
            response.AppendLine($"📞 **Telefon:** {school.Phone}");
            
        if (!string.IsNullOrEmpty(school.Email))
            response.AppendLine($"✉️ **E-post:** {school.Email}");
            
        // Address
        if (school.VisitingAddress != null)
        {
            response.AppendLine($"📍 **Adress:** {school.VisitingAddress.StreetAddress}, " +
                                $"{school.VisitingAddress.PostalCode} {school.VisitingAddress.Locality}");
        }
            
        response.AppendLine(); // Blank line between schools
    }
}