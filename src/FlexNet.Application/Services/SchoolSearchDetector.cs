using FlexNet.Application.Configuration;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services;

public class SchoolSearchDetector : ISchoolSearchDetector
{
   private readonly SchoolSearchConfiguration _config;

   // This might need configuration, if no words matchen in the message then it will not search for any kind of education
   private static readonly string[] SchoolKeywords = new[]
   {
      "skola", "school", "gymnasium", "program", "universitet", "university", "folkhögskola", "komvux",
      "vuxenutbildning", "adult education"
   };
   public SchoolSearchDetector(SchoolSearchConfiguration config, ILogger<SchoolSearchDetector> logger)
   {
      _config = config ??  throw new ArgumentNullException(nameof(config));
   }

public SchoolRequestInfo? DetectSchoolRequest(string message,
    IEnumerable<ConversationMessage>? recentHistory = null)
        {
            // Extract raw message from XML context if present
            var rawMessage = ExtractRawMessage(message);
            var lowerMessage = rawMessage.ToLowerInvariant();
            
            // Quick filter: Does this even mention schools?
            if (!ContainsSchoolKeywords(lowerMessage))
            {
                return null;
            }
            var municipality = ExtractMunicipality(lowerMessage);
            var programCodes = ExtractProgramCodes(lowerMessage);

            if (programCodes is not { Count: 0 } && recentHistory != null)
            {
                programCodes = ExtractProgramCodesFromHistory(recentHistory);
            }
            
            var request = new SchoolRequestInfo
            {
                Municipality = municipality,
                ProgramCodes = programCodes
            };

            // Validate: Must have at least municipality OR program
            if (request.Municipality != null ||
                (request.ProgramCodes != null && request.ProgramCodes.Count != 0)) return request;
            
            return null;

        }
        
        /// Extracts raw message from XML context tags if present.
        private static string ExtractRawMessage(string message)
        {
            // If message contains XML context tags, extract the actual message
            if (!message.Contains("<userMessage>") || !message.Contains("</userMessage>")) return message;
            var start = message.IndexOf("<userMessage>", StringComparison.Ordinal) + "<userMessage>".Length;
            var end = message.IndexOf("</userMessage>", StringComparison.Ordinal);
                
            if (start >= 0 && end > start)
            {
                return message.Substring(start, end - start).Trim();
            }

            return message;
        }
        
        /// Checks if message contains school-related keywords.
        private static bool ContainsSchoolKeywords(string lowerMessage)
        {
            return SchoolKeywords.Any(lowerMessage.Contains);
        }
        
        /// Extracts municipality name from message using keyword matching.
        private string? ExtractMunicipality(string lowerMessage)
        {
            foreach (var (municipality, variants) in _config.Municipalities)
            {
                if (variants.Any(lowerMessage.Contains))
                {
                    return municipality;
                }
            }
            
            return null;
        }
        
        /// Extracts program codes from message using keyword matching.
        private List<string>? ExtractProgramCodes(string lowerMessage)
        {
            var detectedPrograms = new List<string>();
            
            // Add spaces around message for whole-word matching
            var messageWithSpaces = " " + lowerMessage + " ";
            
            foreach (var (code, keywords) in _config.ProgramKeywords)
            {
                if (keywords.Any(keyword => messageWithSpaces.Contains(keyword)))
                {
                    detectedPrograms.Add(code);
                }
            }
            
            return detectedPrograms.Count != 0 ? detectedPrograms : null;
        }
        private List<string>? ExtractProgramCodesFromHistory(IEnumerable<ConversationMessage> history)
        {
            var detectedPrograms = new List<string>();
        
            // Look at last 3 user messages
            var recentUserMessages = history
                .Where(m => m.Role == "user")
                .TakeLast(3)
                .Select(m => m.Content.ToLowerInvariant());
        
            foreach (var message in recentUserMessages)
            {
                var messageWithSpaces = " " + message + " ";
            
                foreach (var (code, keywords) in _config.ProgramKeywords)
                {
                    if (!keywords.Any(keyword => messageWithSpaces.Contains(keyword))) continue;
                    if (!detectedPrograms.Contains(code))
                        detectedPrograms.Add(code);
                }
            }
        
            return detectedPrograms.Count != 0 ? detectedPrograms : null;
        }
}