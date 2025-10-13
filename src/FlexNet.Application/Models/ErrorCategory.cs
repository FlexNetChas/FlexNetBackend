namespace FlexNet.Application.Models;

public enum ErrorCategory
{
    Network,
    ServiceOverloaded,
    RateLimited,
    Authentication,
    InvalidInput,
    Unknown
}