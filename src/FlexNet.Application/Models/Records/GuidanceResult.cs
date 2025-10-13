namespace FlexNet.Application.Models.Records;

public record GuidanceResult(bool IsSuccess, string Content, ErrorInfo? Error)

{
public static GuidanceResult Success(string guidance) => new(true, guidance, null);

public static GuidanceResult Failure(ErrorInfo error) => new(false, string.Empty, error);
}