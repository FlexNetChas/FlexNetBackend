using FlexNet.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

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