namespace TheBooker.Domain.Common.Results;

/// <summary>
/// Represents a validation result that can contain multiple errors.
/// </summary>
public sealed class ValidationResult : Result
{
    private ValidationResult(Error[] errors)
        : base(false, Error.Validation("ValidationError", "One or more validation errors occurred."))
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation result with multiple errors.
    /// </summary>
    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Represents a validation result with a value that can contain multiple errors.
/// </summary>
public sealed class ValidationResult<TValue> : Result<TValue>
{
    private ValidationResult(Error[] errors)
        : base(default, false, Error.Validation("ValidationError", "One or more validation errors occurred."))
    {
        Errors = errors;
    }

    public Error[] Errors { get; } = [];

    /// <summary>
    /// Creates a validation result with multiple errors.
    /// </summary>
    public static ValidationResult<TValue> WithErrors(Error[] errors) => new(errors);
}
