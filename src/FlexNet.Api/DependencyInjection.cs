using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlexNet.Application;
using FlexNet.Infrastructure;

namespace FlexNet.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddAppDI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationDI();

        services.AddInfrastructureDI(configuration);

        return services;
    }
}


