using Azure.Identity;
using FlexNet.Api;
using FlexNet.Api.Configuration;
using FlexNet.Api.Exceptions;
using FlexNet.Api.Middleware;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var keyVaultName = builder.Configuration["KeyVault:VaultName"];
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}

// Remove Server header. No reason to display Kestrel server info and expose to black hats
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Add services to the container.
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();

// Exceptions: Cast cronology. Therefore, more specific exceptions should be registered first and global last
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add all layers through API layer (registers repositories, services, and validators)
builder.Services.AddAppDI(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddRateLimitingConfiguration();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddAiMockClient(builder.Configuration);
var app = builder.Build();

// Simple public health check endpoint to check if the service is running
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    service = "FlexNet API"
})).AllowAnonymous();

// All middlewares is configure in MiddlewareExtensions class
app.ConfigureMiddleware();

app.Run();