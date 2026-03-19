namespace Shared.Kernel;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unexpected
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType = ErrorType.Validation)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error, ErrorType.Validation);
    public static Result NotFound(string error) => new(false, error, ErrorType.NotFound);
    public static Result Conflict(string error) => new(false, error, ErrorType.Conflict);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error, ErrorType.Validation);
    public static Result<T> NotFound<T>(string error) => new(default, false, error, ErrorType.NotFound);
    public static Result<T> Conflict<T>(string error) => new(default, false, error, ErrorType.Conflict);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error, ErrorType errorType = ErrorType.Validation)
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }
}
