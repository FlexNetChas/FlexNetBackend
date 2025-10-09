using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Models;
using Mscc.GenerativeAI;

namespace FlexNet.Infrastructure.Services
{
    public class GeminiGuidanceService : IGuidanceService
    {
        private readonly IApiKeyProvider _apiKeyProvider;

        public GeminiGuidanceService(IApiKeyProvider apiKeyProvider)
        {
            _apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        public async Task<string> GetGuidanceAsync(
            string userMessage,
            IEnumerable<ConversationMessage> conversationHistory,
            StudentContext studentContext)
        {
            var apiKey = await _apiKeyProvider.GetApiKeyAsync();
            var model = new GenerativeModel() { ApiKey = apiKey };
            var response = await model.GenerateContent(userMessage);
            return response.Text;
        }
    }
}