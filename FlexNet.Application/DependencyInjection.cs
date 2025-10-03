using Microsoft.Extensions.DependencyInjection;

namespace FlexNet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // Registrera application services
            return services;
        }
    }
}
