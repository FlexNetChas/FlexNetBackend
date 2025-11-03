using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlexNet.Api.Exceptions
{
    // internal - Only accessible within the Spenvio.Api assembly (not visible to other projects)
    // sealed - Prevents other classes from inheriting this class
    internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            /* Switch status code based on exception type. Needed for handling
               exceptions in problem details. Default to 500 Internal Server Error. 
               Specific exceptions will be handle in seperate classes */
            httpContext.Response.StatusCode = exception switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext, // Extrat information from current request
                Exception = exception, 
                // Build details to expose to the consumer of the API
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occurred while processing your request.",
                    Detail = exception.Message
                }
            });
        }
    }
}
