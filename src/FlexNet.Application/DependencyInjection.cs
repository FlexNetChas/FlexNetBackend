using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using FlexNet.Application.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FlexNet.Application.Configuration;
using FlexNet.Application.Security;
using FlexNet.Application.Services.AiGenerators;
using FlexNet.Application.Services.Factories;

namespace FlexNet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // Registrera application services
            services.AddScoped<SendCounsellingMessage>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IChatSessionService, ChatSessionService>();
            services.AddScoped<IUserDescriptionService, UserDescriptionService>();
            services.AddScoped<IInputSanitizer, InputSanitizer>();
            services.AddScoped<IOutputValidator,  OutputValidator>();
            services.AddScoped<AiContextBuilder>();
            services.AddScoped<ChatMessageCreator>();
            services.AddScoped<ConversationContextbuilder>();
            services.AddSingleton<SchoolSearchConfiguration>();
            services.AddScoped<SchoolSearchDetector>();
            services.AddScoped<TitleGenerator>();
            services.AddScoped<SendCounsellingMessageStreaming>();
            services.AddScoped<MessagePersistence>();
            services.AddScoped<MessageContextPreparation>();
            services.AddScoped<IUserDataExportService, UserDataExportService>();
            services.AddScoped<IPromptEnricher, PromptEnricher>();
            
            /* Register FluentValidation validators from Application assembly.
             * AddValidatorsFromAssembly is an extension method from FluentValidation that scans 
             * Application assembly for all classes that inherit from AbstractValidator<DTO> */
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}