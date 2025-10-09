using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Models;
using Mscc.GenerativeAI;

namespace FlexNet.Infrastructure.Services
{
    public class GeminiGuidanceService : IGuidanceService
    {
        private readonly string _apiKey;

        public GeminiGuidanceService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<string> GetGuidanceAsync(
            string userMessage,
            IEnumerable<ConversationMessage> conversationHistory,
            StudentContext studentContext)
        {
            var model = new GenerativeModel() { ApiKey = _apiKey };
            var response = await model.GenerateContent(userMessage);
            return response.Text;
        }
    }
}