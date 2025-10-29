namespace FlexNet.Api.Middleware;

public static class MiddlewareExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();

        /* Security headers for productions
         * If environment is not development, apply security headers to protect against common vulnerabilities
         * Activate HSTS to enforce HTTPS. Forwarded headers for rate limiting. Enable response compression for performance.
         * We could add more security headers here as needed
         */
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseForwardedHeaders();
            app.UseResponseCompression();
        }
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}