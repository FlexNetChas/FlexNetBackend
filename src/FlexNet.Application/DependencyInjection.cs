using FlexNet.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;
using Microsoft.Extensions.Logging;


namespace FlexNet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // Registrera application services
            services.AddScoped<SendCounsellingMessage>();
    
            return services;
        }
    }
}