using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlexNet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add entity framework, identity, and other infrastructure services here

            // Ex:
            // services.AddDbContext<AppDbContext>(options =>
            //     options.UseSqlServer(configuration.GetConnectionString("NetFlex-connection-string")));

            return services;
        }
    }
}
