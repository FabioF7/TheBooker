namespace TheBooker.Domain.Common.Results;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Implements the Result Pattern to avoid exceptions for control flow.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a result based on a condition.
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    /// Creates a result with value based on a condition.
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue? value, Error error) =>
        value is not null ? Success(value) : Failure<TValue>(error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// Throws if accessed on a failed result.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Implicit conversion from error to failed result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
