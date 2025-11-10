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
                          "http://flexnet-frontend:3000",       // Docker Compose internal
                          "https://690dcda9bfa55eda4f317475--flexnetfrontend.netlify.app" // Netlify Frontend Next.js
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
