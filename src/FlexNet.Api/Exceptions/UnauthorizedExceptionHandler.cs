using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlexNet.Api.Exceptions
{
    // Handles UnauthorizedAccessException and maps it to HTTP 401 Unauthorized
    internal sealed class UnauthorizedExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not UnauthorizedAccessException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var context = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "You do not have permission to access this resource.",
                    Status = StatusCodes.Status401Unauthorized
                }
            };

            return await problemDetailsService.TryWriteAsync(context);
        }
    }
}
