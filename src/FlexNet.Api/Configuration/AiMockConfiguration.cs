using FlexNet.Application.Interfaces.IServices;
using FlexNet.Infrastructure.Services.Gemini;
using FlexNet.Infrastructure.Services.Mock;

namespace FlexNet.Api.Configuration
{
    public static class AiClientConfiguration
    {
        /// Registers AI client based on environment.
        /// Reads ASPNETCORE_ENVIRONMENT to decide mock vs real client.
        public static IServiceCollection AddAiMockClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
            
            if (isDevelopment)
            {
                RegisterMockClient(services, configuration);
            }
            else
            {
                RegisterGeminiClient(services);
            }
            
            return services;
        }

        private static void RegisterMockClient(IServiceCollection services, IConfiguration configuration)
        {
            var minDelay = configuration.GetValue<int>("MockAi:MinDelayMs", 100);
            var maxDelay = configuration.GetValue<int>("MockAi:MaxDelayMs", 500);
            
            services.AddScoped<IAiClient>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<MockAiClient>>();
                return new MockAiClient(logger, minDelay, maxDelay);
            });
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║  🎭 DEVELOPMENT MODE: Mock AI Client              ║");
            Console.WriteLine("║                                                  ║");
            Console.WriteLine("║  ✓ No real API calls                             ║");
            Console.WriteLine("║  ✓ No rate limits                                ║");
            Console.WriteLine("║  ✓ Instant responses ({0}-{1}ms)                 ║", minDelay, maxDelay);
            Console.WriteLine("║  ✓ Works offline                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void RegisterGeminiClient(IServiceCollection services)
        {
            services.AddScoped<IAiClient, GeminiApiClient>();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║  🤖 PRODUCTION MODE: Real Gemini AI Client        ║");
            Console.WriteLine("║                                                  ║");
            Console.WriteLine("║  ✓ Live API calls                                ║");
            Console.WriteLine("║  ⚠ Rate limits apply                             ║");
            Console.WriteLine("║  ⚠ API costs apply                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}