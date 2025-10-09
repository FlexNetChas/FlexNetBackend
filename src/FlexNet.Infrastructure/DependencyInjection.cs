using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FlexNet.Application.Interfaces;
using FlexNet.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlexNet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add entity framework, identity, and other infrastructure services here

            // Ex:
            // services.AddDbContext<AppDbContext>(options =>
            //     options.UseSqlServer(configuration.GetConnectionString("NetFlex-connection-string")));
            services.AddScoped<IGuidanceService, GeminiGuidanceService>();

            string vaultName = configuration["KEY_VAULT_NAME"] ?? Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            if (!string.IsNullOrWhiteSpace(vaultName))
            {
                var vaultUri = new Uri($"https://{vaultName}.vault.azure.net");
                services.AddSingleton(new SecretClient(vaultUri, new DefaultAzureCredential()));
                services.AddMemoryCache();
                services.AddSingleton<IApiKeyProvider, KeyVaultApiKeyProvider>();
            }
            return services;
        }
    }
}