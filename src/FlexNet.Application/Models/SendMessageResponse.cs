namespace FlexNet.Application.Models;

public class SendMessageResponse
{
    public string Reply { get; set; }
    public bool IsSuccess { get; set; }

    public string? ErrorCode { get; set; }
    public bool CanRetry { get; set; }
    public int? RetryAfter { get; set; }
}