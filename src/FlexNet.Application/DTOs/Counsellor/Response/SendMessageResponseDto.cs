namespace FlexNet.Application.DTOs.Counsellor.Response;

public record SendMessageResponseDto(
    string Reply,
    bool IsSuccess,
    string? ErrorCode,
    bool CanRetry,
    int? RetryAfter
);