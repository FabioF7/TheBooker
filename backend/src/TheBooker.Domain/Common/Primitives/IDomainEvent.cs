using MediatR;

namespace TheBooker.Domain.Common.Primitives;

/// <summary>
/// Marker interface for domain events.
/// Implements INotification for MediatR integration.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}

/// <summary>
/// Base implementation for domain events with common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
