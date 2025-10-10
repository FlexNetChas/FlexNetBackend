using FlexNet.Application.Models;

namespace FlexNet.Application.Exceptions;

public class ServiceException : Exception
{
    public ErrorCategory Category { get; }
    public bool CanRetry { get; }
    public string UserMessage { get; }
    public string ErrorCode { get; }
    public int? HttpStatusCode { get; }
    public TimeSpan? RetryAfter { get; }

    private ServiceException(string message, ErrorCategory category, bool canRetry, string errorCode, string userMessage,
        int? httpStatusCode = null, TimeSpan? retryAfter = null, Exception? innerException = null) : base(message,
        innerException)
    {
        Category = category;
        CanRetry = canRetry;
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
        RetryAfter = retryAfter;
        UserMessage = userMessage;
    }

    public static ServiceException NetworkError(string message, Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.Network,
            canRetry: true,
            errorCode: "NETWORK_ERROR",
            userMessage: "We're having trouble connecting to the server. Please try again in a moment.",
            httpStatusCode: null,
            retryAfter: null,
            innerException: innerException);
    }

    public static ServiceException ServiceOverloaded(string message, TimeSpan? retryAfter = null,
        Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.ServiceOverloaded,
            canRetry: true,
            errorCode: "SERVICE_OVERLOADED",
            userMessage: "A lot of people are using the counsellor right now. Please try again in a moment.",
            httpStatusCode: 503,
            retryAfter: retryAfter,
            innerException: innerException);
    }
    public static ServiceException RateLimited(string message,
        TimeSpan? retryAfter = null, Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.RateLimited,
            canRetry: true,
            errorCode: "RATE_LIMITED",
            userMessage:"You're sedning messages a bit too quickly. Let's take a short break and try again in a moment. ",
            httpStatusCode: 429,
            retryAfter: retryAfter,
            innerException: innerException
            );
    }
    
    public static ServiceException AuthenticationFailed(
        string message,
        Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.Authentication,
            canRetry: false,
            errorCode: "AUTHENTICATION_ERROR",
            userMessage: "We're experiencing technical difficulties. Our team has been notified.",
            httpStatusCode: 401,
            retryAfter: null,
            innerException: innerException);
    }
    
    public static ServiceException InvalidInput(
        string message,
        Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.InvalidInput,
            canRetry: false,
            errorCode: "INPUT_ERROR",
            userMessage: "Invalid input, need to include a message.",
            httpStatusCode: 400,
            retryAfter: null,
            innerException: innerException);
    }
    
    public static ServiceException Unknown(
        string message,
        Exception? innerException = null)
    {
        return new ServiceException(
            message,
            ErrorCategory.Unknown,
            canRetry: false,
            errorCode: "UNKNOWN_ERROR",
            userMessage: "Unknown, please try again.",
            httpStatusCode: 500,
            retryAfter: null,
            innerException: innerException);
    }
}