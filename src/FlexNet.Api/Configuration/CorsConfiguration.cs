namespace FlexNet.Api.Configuration
{
    public static class CorsConfiguration
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                          "http://localhost:3000",       // Local development
                          "http://host.docker.internal:3000",   // Docker Desktop
                          "http://flexnet-frontend:3000"        // Docker Compose internal
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
            return services;
        }
    }
}
