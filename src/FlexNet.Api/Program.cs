using FlexNet.Api;
using FlexNet.Api.Configuration;
using FlexNet.Api.Exceptions;
using FlexNet.Api.Middleware;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// All middlewares is configure in MiddlewareExtensions class
app.ConfigureMiddleware();

app.Run();