using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FlexNet.Api.Configuration;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("authenticated-counsellor", context =>
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("sub")?.Value
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
            options.AddConcurrencyLimiter("global-quota", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfter = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTimeSpan))
                {
                    retryAfter = (int)retryAfterTimeSpan.TotalSeconds;
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "You're sending messages to quickly. Please take a moment before trying again.",
                    errorCode = "RATE_LIMITED",
                    canRetry = true,
                    retryAfter = retryAfter
                }, cancellationToken: cancellationToken);
            };
        });
        return services;
    }
    
        
    
}