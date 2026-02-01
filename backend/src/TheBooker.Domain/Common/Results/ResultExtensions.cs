namespace TheBooker.Domain.Common.Results;

/// <summary>
/// Extension methods for Result pattern to enable functional composition.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the value of a successful result using the provided function.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds the result to another operation that returns a Result.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Matches the result to one of two functions based on success/failure.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public static Result<TValue> Tap<TValue>(
        this Result<TValue> result,
        Action<TValue> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    /// Ensures a condition is met on the value, failing with the provided error if not.
    /// </summary>
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Combines multiple results, returning the first failure or success.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Converts a nullable value to a Result.
    /// </summary>
    public static Result<TValue> ToResult<TValue>(
        this TValue? value,
        Error error) where TValue : class
    {
        return value is not null
            ? Result.Success(value)
            : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Converts a nullable struct value to a Result.
    /// </summary>
    public static Result<TValue> ToResult<TValue>(
        this TValue? value,
        Error error) where TValue : struct
    {
        return value.HasValue
            ? Result.Success(value.Value)
            : Result.Failure<TValue>(error);
    }
}
