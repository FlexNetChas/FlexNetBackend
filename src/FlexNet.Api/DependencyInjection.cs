using FlexNet.Application;
using FlexNet.Infrastructure;

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
