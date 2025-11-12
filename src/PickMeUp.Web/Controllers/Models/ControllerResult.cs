using System.Diagnostics.CodeAnalysis;
using PickMeUp.Web.Infrastructure;

namespace PickMeUp.Web.Controllers;

public readonly struct ControllerResult
{
    /// <summary>
    /// Inidicates whether the result has a non-success status code.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Will return null. This will allow for a non-generic result.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Alert messages.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Create a new result with a success status code.
    /// </summary>
    public static ControllerResult Success(object? data = null)
        => new()
        {
            IsSuccess = true,
            Data = data
        };

    /// <summary>
    /// Create a new result with error status code and the provided error message.
    /// </summary>
    public static ControllerResult Error(string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}