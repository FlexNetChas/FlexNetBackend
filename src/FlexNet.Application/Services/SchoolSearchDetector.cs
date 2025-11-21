using FlexNet.Application.Configuration;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using FlexNet.Domain.Entities.Schools;
using Microsoft.Extensions.Logging;

namespace FlexNet.Application.Services;

public class SchoolSearchDetector : ISchoolSearchDetector
{
   private readonly SchoolSearchConfiguration _config;
   private readonly IProgramService _service;
   private List<SchoolProgram>? _cachedPrograms;

   // This might need configuration, if no words matchen in the message then it will not search for any kind of education
   private static readonly string[] SchoolKeywords = new[]
   {
      "skola", "school", "gymnasium", "program", "universitet", "university", "folkhögskola", "komvux",
      "vuxenutbildning", "adult education"
   };
   public SchoolSearchDetector(SchoolSearchConfiguration config, ILogger<SchoolSearchDetector> logger, IProgramService service)
   {
       _config = config ??  throw new ArgumentNullException(nameof(config));
       _service = service ??  throw new ArgumentNullException(nameof(service));
   }

    public async Task<SchoolRequestInfo?> DetectSchoolRequest(string message,
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
            var programCodes = await ExtractProgramCodes(lowerMessage);
            
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
        private async Task<List<string>?> ExtractProgramCodes(string lowerMessage)
        {
            var programs = await GetProgramsAsync();
            var detectedPrograms = new List<string>();
            
            // Add spaces around message for whole-word matching
            var messageWithSpaces = " " + lowerMessage + " ";
            
            foreach (var program in programs)
            {
                var programNameLower = program.Name.ToLowerInvariant();
        
                if (messageWithSpaces.Contains(programNameLower) || 
                    programNameLower.Contains(lowerMessage.Trim()))
                {
                    detectedPrograms.Add(program.Code);
                }
            }
            
            return detectedPrograms.Count != 0 ? detectedPrograms : null;
        }

        private async Task<List<SchoolProgram>> GetProgramsAsync()
        {
            if (_cachedPrograms != null)
                return _cachedPrograms;

            var result = await _service.GetAllProgramsAsync();
            if (!result.IsSuccess || result.Data == null) return [];
            _cachedPrograms = result.Data.ToList();
            return _cachedPrograms;

        }
}