namespace Auth.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public string Message { get; set; } = string.Empty;

    public static Result Success() => new(true, null);
    public static Result Success(string message) => new(true, null) { Message = message };
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Success<T>(T value, string message) => new(value, true, null) { Message = message };
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _data;

    internal Result(T? data, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        _data = data;
    }

    public T Data => IsSuccess
        ? _data!
        : default;

    public static implicit operator Result<T>(T value) => new(value, true, null);
}
