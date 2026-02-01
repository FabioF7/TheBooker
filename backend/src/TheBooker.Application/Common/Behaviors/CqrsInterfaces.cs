using MediatR;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Application.Common.Behaviors;

/// <summary>
/// Base interface for CQRS queries with Result pattern.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Base interface for CQRS commands with Result pattern.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Base interface for CQRS commands that return a value.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
