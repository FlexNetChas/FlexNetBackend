using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;

namespace FlexNet.Api.Configuration;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("authenticated-counsellor", context =>
            {
                var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            /* Global quota for all authenticated users to prevent abuse of the API
             * Each authenticated user can have up to 100 requests */
            options.AddConcurrencyLimiter("global-quota", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            /* Limit login attempts to 10 per minute per IP address
             * This rate limiting protect against brute-force attacks on the login/registration endpoints
             * We could implement more controll later such as account lockout after several failed attempts */
            options.AddPolicy("public-auth", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfter = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTimeSpan))
                {
                    retryAfter = (int)retryAfterTimeSpan.TotalSeconds;
                }

                // Dynamic error message based on which endpoint was hit
                var path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
                var message = path switch
                {
                    _ when path.EndsWith("/auth/login")
                        => "Too many login attempts. Please wait a moment before trying again.",
                    _ when path.EndsWith("/auth/register")
                        => "Too many registration attempts. Please wait a moment before trying again.",
                    _ when path.EndsWith("/auth/refresh")
                        => "Too many token refresh attempts. Please wait a moment before trying again.",
                    _ when path.Contains("/counsellor/")
                        => "You're sending messages too quickly. Please take a moment before trying again.",
                    _ => "Too many requests. Please try again later."
                };

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = message,
                    errorCode = "RATE_LIMITED",
                    canRetry = true,
                    retryAfter = retryAfter
                }, cancellationToken: cancellationToken);
            };
        });
        return services;
    }
}