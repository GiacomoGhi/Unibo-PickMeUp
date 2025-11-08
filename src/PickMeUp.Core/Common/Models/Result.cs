using System.Diagnostics.CodeAnalysis;

namespace PickMeUp.Core.Common.Models;

public enum ResultStatus
{
    Success,
    Error,
    InvalidArgument,
    Forbidden,
    NotFound,
    Unauthorized,
}

public readonly struct Result
{
    /// <summary>
    /// Inidicates whether the result has a non-success status code.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool HasNonSuccessStatusCode => StatusCode != ResultStatus.Success;

    /// <summary>
    /// Status code that indicates the result of the operation.
    /// </summary>
    public ResultStatus StatusCode { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Will return null. This will allow for a non-generic result.
    /// </summary>
    public object? Data => null;

    /// <summary>
    /// Create a new result with a success status code.
    /// </summary>
    public static Result Success()
        => new()
        {
            StatusCode = ResultStatus.Success
        };

    /// <summary>
    /// Create a new result with a success status code and the provided data.
    /// </summary>
    public static Result<T> Success<T>(T data)
        where T : class
        => new()
        {
            Data = data,
            StatusCode = ResultStatus.Success
        };

    /// <summary>
    /// Create a new result with error status code and the provided error message.
    /// </summary>
    public static Result Error(string msg)
        => new()
        {
            StatusCode = ResultStatus.Error,
            ErrorMessage = msg
        };

    /// <summary>
    /// Create a new result with error status code and the provided error message.
    /// </summary>
    public static Result InvalidArgument(string msg)
        => new()
        {
            StatusCode = ResultStatus.InvalidArgument,
            ErrorMessage = $"Invalid argument: {msg}"
        };

    /// <summary>
    /// Create a new result with forbidden status code.
    /// </summary>
    public static Result Forbidden()
        => new()
        {
            StatusCode = ResultStatus.Forbidden,
            ErrorMessage = "Operation not allowed"
        };

    /// <summary>
    /// Create a new result with unauthorized status code.
    /// </summary>
    public static Result Unauthorized()
        => new()
        {
            StatusCode = ResultStatus.Unauthorized,
            ErrorMessage = "Unauthorized access"
        };

    /// <summary>
    /// Create a new result with not found status code and the provided error message.
    /// </summary>
    public static Result NotFound(string msg)
        => new()
        {
            StatusCode = ResultStatus.NotFound,
            ErrorMessage = $"Item not found: {msg}"
        };
}

public readonly struct Result<T>
    where T : class
{
    /// <inheritdoc cref="Result.HasNonSuccessStatusCode"/>
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public readonly bool HasNonSuccessStatusCode => StatusCode != ResultStatus.Success;

    /// <inheritdoc cref="Result.StatusCode"/>
    public ResultStatus StatusCode { get; init; }

    /// <inheritdoc cref="Result.ErrorMessage"/>
    public string? ErrorMessage { get; init; }

    /// <inheritdoc/>
    public T? Data { get; init; }

    /// <summary>
    /// Implicitly convert a Result to a Result<T>.
    /// </summary>
    public static implicit operator Result<T>(Result result)
    {
        return new Result<T>
        {
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage,
            Data = null
        };
    }

    /// <summary>
    /// Create a new result with a success status code and the provided data.
    /// </summary>
    public static Result<T> Success(T data)
        => new()
        {
            StatusCode = ResultStatus.Success,
            Data = data,
        };

    /// <summary>
    /// Create a new result with error status code and the provided error message.
    /// </summary>
    public static Result<T> Error(string msg)
        => new()
        {
            StatusCode = ResultStatus.Error,
            ErrorMessage = msg,
        };

    /// <summary>
    /// Create a new result with invalid arguments status code and the provided error message.
    /// </summary>
    public static Result<T> InvalidArgument(string msg)
        => new()
        {
            StatusCode = ResultStatus.InvalidArgument,
            ErrorMessage = $"Invalid argument: {msg}",
        };

    /// <summary>
    /// Create a new result with forbidden status code.
    /// </summary>
    public static Result<T> Forbidden()
        => new()
        {
            StatusCode = ResultStatus.Forbidden,
            ErrorMessage = "Operation not allowed",
        };

    /// <summary>
    /// Create a new result with not found status code and the provided error message.
    /// </summary>
    public static Result<T> NotFound(string msg)
        => new()
        {
            StatusCode = ResultStatus.NotFound,
            ErrorMessage = $"Item not found: {msg}",
        };
}