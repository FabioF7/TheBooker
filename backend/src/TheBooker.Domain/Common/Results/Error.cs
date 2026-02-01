namespace TheBooker.Domain.Common.Results;

/// <summary>
/// Represents a domain error with code and description.
/// Uses modern record syntax for immutability and value semantics.
/// </summary>
public sealed record Error(string Code, string Description)
{
    /// <summary>
    /// Represents no error (success state).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string code, string description) =>
        new($"Validation.{code}", description);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(string entityName, Guid id) =>
        new($"NotFound.{entityName}", $"{entityName} with ID '{id}' was not found.");

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(string code, string description) =>
        new($"Conflict.{code}", description);

    /// <summary>
    /// Creates a failure error.
    /// </summary>
    public static Error Failure(string code, string description) =>
        new($"Failure.{code}", description);

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(Error error) => error.Code;

    public override string ToString() => $"{Code}: {Description}";
}
