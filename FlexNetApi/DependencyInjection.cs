using FlexNet.Application;
using FlexNet.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlexNet.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddApplicationDI()
                .AddInfrastructureDI(configuration);

            return services;
        }
    }
}
