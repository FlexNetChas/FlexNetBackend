namespace FlexNet.Application.Models.Records;

public record ErrorInfo(
    string ErrorCode,
    bool CanRetry,
    int? RetryAfter,
    string? Message);