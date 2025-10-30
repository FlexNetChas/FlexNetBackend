namespace FlexNet.Application.Models.Records;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ErrorInfo? Error { get; init; }

    private Result(bool isSuccess, T? data, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static Result<T> Success(T data)
    {
        return data == null ? throw new ArgumentNullException(nameof(data), "success result cannot have null data. Use Failure for errors.") : new Result<T>(true, data, null);
    }

    public static Result<T> Failure(ErrorInfo error)
    {
        return error == null ? throw new ArgumentNullException(nameof(error), "Failure result must have error information.") : new Result<T>(false, default, error);
    }

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Data != null)
            action(Data);
        return this;
    }

    public Result<T> OnFailure(Action<ErrorInfo> action)
    {
        if (!IsSuccess && Error != null)
            action(Error);
        return this;
    }
}