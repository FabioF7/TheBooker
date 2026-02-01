using TheBooker.Domain.Common.Results;

namespace TheBooker.Api.Common;

/// <summary>
/// Extension methods for Result pattern to HTTP responses.
/// </summary>
public static class ResultExtensions
{
    public static IResult Match<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess,
        Func<Error, IResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : Results.Problem(
                detail: result.Error.Description,
                statusCode: GetStatusCode(result.Error));
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(
                detail: result.Error.Description,
                statusCode: GetStatusCode(result.Error));
    }

    private static int GetStatusCode(Error error) => error.Code switch
    {
        var code when code.StartsWith("NotFound") => 404,
        var code when code.StartsWith("Validation") => 400,
        var code when code.StartsWith("Conflict") => 409,
        var code when code.StartsWith("Unauthorized") => 401,
        var code when code.StartsWith("Forbidden") => 403,
        _ => 500
    };
}
