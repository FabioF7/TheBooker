using FluentValidation;
using MediatR;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for validation using FluentValidation.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
                .ToArray();

            // Return validation failure
            // This requires creating the appropriate Result type
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)ValidationResult.WithErrors(errors);
            }

            // For Result<T>, we need to use reflection or a different approach
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = resultType.GetGenericArguments()[0];
                var validationResultType = typeof(ValidationResult<>).MakeGenericType(valueType);
                var withErrorsMethod = validationResultType.GetMethod("WithErrors");
                return (TResponse)withErrorsMethod!.Invoke(null, new object[] { errors })!;
            }
        }

        return await next();
    }
}
