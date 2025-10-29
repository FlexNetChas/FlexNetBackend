using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlexNet.Api.Exceptions
{
    // Handles KeyNotFoundException and maps it to HTTP 404 Not Found
    internal sealed class NotFoundExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not KeyNotFoundException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            var context = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Title = "Resource Not Found",
                    Detail = exception.Message,
                    Status = StatusCodes.Status404NotFound
                }
            };

            return await problemDetailsService.TryWriteAsync(context);
        }
    }
}
