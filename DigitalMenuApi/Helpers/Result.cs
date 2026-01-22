namespace DigitalMenuApi.Helpers;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    // Private constructor - force factory methods
    private Result(bool isSuccess, T? data, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    // Factory methods
    public static Result<T> Success(T data) => new(true, data, null, 200);
    public static Result<T> Failure(string error, int statusCode = 400) => new(false, default, error, statusCode);
    public static Result<T> Unauthorized(string error = "Unauthorized") => new(false, default, error, 401);
    public static Result<T> Forbidden(string error = "Forbidden") => new(false, default, error, 403);
    public static Result<T> NotFound(string error = "Not found") => new(false, default, error, 404);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null, 200);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result Unauthorized(string error = "Unauthorized") => new(false, error, 401);
    public static Result Forbidden(string error = "Forbidden") => new(false, error, 403);
    public static Result NotFound(string error = "Not found") => new(false, error, 404);
}