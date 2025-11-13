using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services.AiGenerators;

public class NoResultsGenerator : INoResultsGenerator
{
   private readonly IAiClient  _aiClient;
   private readonly ILogger<NoResultsGenerator> _logger;
   public NoResultsGenerator(IAiClient aiClient, ILogger<NoResultsGenerator> logger)
   {
      _aiClient = aiClient ??  throw new ArgumentNullException(nameof(aiClient));
      _logger = logger ??  throw new ArgumentNullException(nameof(logger));
   }

   public async Task<Result<string>> GenerateAsync(string userMessage, SchoolRequestInfo searchCriteria,
      UserContextDto userContextDto)
   {
      // 1. Build prompt
      var prompt = BuildPrompt(userMessage, searchCriteria, userContextDto);
      
      // 2. Call API
      var result =  await _aiClient.CallAsync(prompt);
      
      // 3. Handle result
      if (result.IsSuccess) return result;
      
      // 4. If API fails, use fallback
      _logger.LogWarning("Failed to generate counseling response: {Error}", result.Error?.Message);
      return GetFallbackMessage();
   }

   private static string BuildPrompt(string userMessage, SchoolRequestInfo request,
      UserContextDto userContextDto)
   {
              var prompt = new StringBuilder();
              
              prompt.AppendLine($"En {userContextDto.Age}-årig elev frågade: '{userMessage}'");
              prompt.AppendLine();
              prompt.AppendLine("Jag sökte i Skolverkets databas men hittade inga skolor som matchar:");
              
              if (!string.IsNullOrEmpty(request.Municipality))
                  prompt.AppendLine($"- Kommun: {request.Municipality}");

              bool? any = request.ProgramCodes.Any();

              if (any == true)
                  prompt.AppendLine($"- Program: {string.Join(", ", request.ProgramCodes)}");
              
              prompt.AppendLine();
              prompt.AppendLine("Hjälp eleven på svenska genom att:");
              prompt.AppendLine("1. Föreslå närliggande kommuner");
              prompt.AppendLine("2. Fråga om de kan överväga relaterade program");
              prompt.AppendLine("3. Uppmuntra dem att specificera sina sökkriterier");
              prompt.AppendLine();
              prompt.AppendLine("Var stödjande och konstruktiv.");
              
              return  prompt.ToString();

   }
   private static Result<string> GetFallbackMessage()
   {
      const string fallback = "Tyvärr hittade jag inga skolor som matchar dina kriterier just nu. " +
                              "Kan du prova att söka i en närliggande kommun eller överväga relaterade program? " +
                              "Jag hjälper gärna till att hitta alternativ!";

      return Result<string>.Success(fallback);
   }
}