using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using FlexNet.Infrastructure.Data;
using FlexNet.Infrastructure.Repositories;
using FlexNet.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlexNet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IUserRepo, UserRepository>();

        // Add Guidance service
        services.AddScoped<GeminiGuidanceService>();
        services.AddScoped<IGuidanceService>(provider =>
        {
            var geminiService = provider.GetRequiredService<GeminiGuidanceService>();
            var logger = provider.GetRequiredService<ILogger<GuidanceService>>();
            return new GuidanceService(geminiService, logger);
        });
        // Add Key Vault + API Key Provider
        string vaultName = configuration["KEY_VAULT_NAME"]
                           ?? Environment.GetEnvironmentVariable("KEY_VAULT_NAME");

        if (string.IsNullOrWhiteSpace(vaultName))
            throw new InvalidOperationException(
                "Missing KEY_VAULT_NAME configuration. Please set it in your appsettings or environment variables.");

        var vaultUri = new Uri($"https://{vaultName}.vault.azure.net");
        services.AddSingleton(new SecretClient(vaultUri, new DefaultAzureCredential()));
        services.AddMemoryCache();
        services.AddSingleton<IApiKeyProvider, KeyVaultApiKeyProvider>();

        return services;
    }
}