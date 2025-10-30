using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FlexNet.Application.Interfaces;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using FlexNet.Infrastructure.Data;
using FlexNet.Infrastructure.Interfaces;
using FlexNet.Infrastructure.Repositories;
using FlexNet.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FlexNet.Infrastructure.Security;
using FlexNet.Infrastructure.Services.Skolverket;

namespace FlexNet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IUserRepo, UserRepository>();
        services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IUserDescriptionRepo, UserDescriptionRepository>();
        services.AddScoped<IChatSessionRepo, ChatSessionRepo>();

        // Add User Context Service
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextService, ExtractUserIdService>();

        // Add Guidance service
        services.AddScoped<GeminiGuidanceService>();
        services.AddScoped<IGuidanceService>(provider =>
        {
            var geminiService = provider.GetRequiredService<GeminiGuidanceService>();
            var logger = provider.GetRequiredService<ILogger<GuidanceService>>();
            return new GuidanceService(geminiService, logger);
        });

        ConfigureSkolverketServices(services, configuration);
        ConfigureKeyVault(services, configuration);

        return services;
    }
    private static void ConfigureSkolverketServices(
        IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHttpClient<ISkolverketApiClient, SkolverketApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.skolverket.se/skolenhetsregistret/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    
        services.AddSingleton<SkolverketMapper>();
        services.AddScoped<ISchoolService, SkolverketSchoolService>();
    
    }
    private static void ConfigureKeyVault(IServiceCollection services, IConfiguration configuration)
    {
        string? vaultName = configuration["KeyVault:VaultName"];
        
        if (string.IsNullOrWhiteSpace(vaultName))
            throw new InvalidOperationException(
                "Missing KeyVault:VaultName configuration. " +
                "For local development: " +
                "1) Run 'az login' to authenticate with Azure, " +
                "2) Run 'dotnet user-secrets set \"KeyVault:VaultName\" \"your-vault-name\"' in the Api project folder.");
        
        var vaultUri = new Uri($"https://{vaultName}.vault.azure.net");
        services.AddSingleton(new SecretClient(vaultUri, new DefaultAzureCredential()));
        services.AddMemoryCache();
        services.AddSingleton<IApiKeyProvider, KeyVaultApiKeyProvider>();
    }
}
