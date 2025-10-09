using FlexNet.API;
using FlexNet.Application.Interfaces;
using FlexNet.Application.UseCases;
using FlexNet.Infrastructure.Services;

namespace FlexNetBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add dependency injection from other layers
            builder.Services.AddAppDI(builder.Configuration);
            var geminiApiKey = builder.Configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
            builder.Services.AddScoped<IGuidanceService>(sp => new GeminiGuidanceService(geminiApiKey));
            builder.Services.AddScoped<SendCounsellingMessage>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}