using Microsoft.Extensions.DependencyInjection;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Application.Services;

namespace FlexNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        // Add Services
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
