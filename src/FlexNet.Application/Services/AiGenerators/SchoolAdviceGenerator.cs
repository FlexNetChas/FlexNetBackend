using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models.Records;
using FlexNet.Application.Services.Formatters;
using FlexNet.Domain.Entities.Schools;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.AiGenerators;

public class SchoolAdviceGenerator : ISchoolAdviceGenerator
{
   private readonly ILogger<SchoolAdviceGenerator> _logger;
   private readonly SchoolResponseFormatter _formatter;
   private readonly IAiClient  _aiClient;
    private static readonly string[] SourceArray = ["skola", "gymnasium", "studera", "plugga", "utbildning"];

    public SchoolAdviceGenerator(ILogger<SchoolAdviceGenerator> logger, SchoolResponseFormatter formatter, IAiClient apiClient)
   {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _formatter = formatter ??  throw new ArgumentNullException(nameof(formatter));
      _aiClient = apiClient ??  throw new ArgumentNullException(nameof(apiClient));
   }

   public async Task<Result<string>> GenerateAdviceAsync(string userMsg, List<School> schools,
      UserContextDto userContext)
   {
      try
      {
         // 1. Build prompt
         var prompt = BuildPrompt(userMsg, schools, userContext);
            
         // 2. Call API
         var result = await _aiClient.CallAsync(prompt);
            
         // 3. Handle result
         if (!result.IsSuccess)  
         {
            _logger.LogWarning("Failed to generate school advice: {Error}", result.Error?.Message);
            return await GetFallbackAdvice(userMsg, userContext);  
         }
            
         // 4. Format with schools (on SUCCESS)
         var formatted = _formatter.FormatSchoolList(result.Data, schools);
         return Result<string>.Success(formatted);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error generating school advice");
         return await GetFallbackAdvice(userMsg, userContext);
      }
   }

   private static string BuildPrompt(string userMsg, List<School> schools, UserContextDto userContext)
   {
      var prompt = new StringBuilder();
      prompt.AppendLine($"En {userContext.Age}-årig elev frågade: '{userMsg}'");
      prompt.AppendLine();
      prompt.AppendLine($"Jag har visat dem {schools.Count} gymnasieskolor från Skolverkets officiella register:");
        
      // Give AI contextDto about which schools
      foreach (var school in schools.Take(3))
      {
         prompt.AppendLine($"- {school.Name} i {school.Municipality}");
      }
        
      prompt.AppendLine();
      prompt.AppendLine("Skriv 3-4 meningar på svenska som:");
      prompt.AppendLine("1. Bekräftar deras intresse för teknik/utbildning");
      prompt.AppendLine("2. Uppmuntrar dem att besöka skolornas webbsidor");
      prompt.AppendLine("3. Föreslår att gå på öppet hus-dagar");
      prompt.AppendLine("4. Erbjuder hjälp med fler frågor");
      prompt.AppendLine();
      prompt.AppendLine("Var varm, stödjande och uppmuntrande.");
      prompt.AppendLine("Skriv ENDAST uppmuntran-texten (inga listor, inga skolnamn).");
        
      return prompt.ToString();
  
   }

   private async Task<Result<string>> GetFallbackAdvice(string userMsg, UserContextDto userContextDto)
   {
      var rawMsg = ExtractRawMessage(userMsg);
    
      // Check if asking about schools but vague
      var isSchoolQuery = SourceArray.Any(k => rawMsg.Contains(k, StringComparison.InvariantCultureIgnoreCase));

      var prompt =
         // Regular counseling
         // Give AI contextDto to ask the right questions
         isSchoolQuery ? $"""
                          En {userContextDto.Age}-årig elev frågade: '{rawMsg}'

                          Eleven är intresserad av gymnasieutbildning men har inte varit specifik ännu.

                          För att kunna söka i skolregistret behöver jag veta:
                          - Vilket ämnesområde/program (t.ex. teknik, naturvetenskap, ekonomi)
                          - Vilken stad/kommun (t.ex. Stockholm, Uppsala, Göteborg)

                          Ställ EN vänlig fråga på svenska för att förstå deras intressen bättre.
                          Var varm och uppmuntrande.
                          """ : userMsg;
    
      var result = await _aiClient.CallAsync(prompt);
      return result;
   }
   private static string ExtractRawMessage(string message)
   {
      if (!message.Contains("<current_message>")) return message;
      const string startTag = "<current_message>";
      const string endTag = "</current_message>";
            
      var startIndex = message.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
      var endIndex = message.IndexOf(endTag, StringComparison.Ordinal);
            
      if (startIndex > 0 && endIndex > startIndex)
      {
         return message.Substring(startIndex, endIndex - startIndex).Trim();
      }

      return message;
   }
}