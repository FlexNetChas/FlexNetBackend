using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace FlexNet.Api.Exceptions
{
    internal sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        // Handles ValidationException and maps it to HTTP 400 Bad Request
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException validationException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var context = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Detail = "One or more validation errors occurred.",
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                }
            };

            // Make sure to expose the validation errors in a structured way
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key.ToLowerInvariant(),
                    g => g.Select(e => e.ErrorMessage).ToArray());
            context.ProblemDetails.Extensions.Add("errors", errors);

            return await problemDetailsService.TryWriteAsync(context);
        }
    }
}
