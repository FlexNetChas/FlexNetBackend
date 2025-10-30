using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using FlexNet.Application.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

            services.AddScoped<AiContextBuilder>();

            /* Register FluentValidation validators from Application assembly.
             * AddValidatorsFromAssembly is an extension method from FluentValidation that scans 
             * Application assembly for all classes that inherit from AbstractValidator<DTO> */
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}