using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Domain.Events;

/// <summary>
/// Event raised when a new tenant is created.
/// </summary>
public sealed record TenantCreatedEvent(Guid TenantId, string Slug) : DomainEventBase;

/// <summary>
/// Event raised when tenant settings are updated.
/// </summary>
public sealed record TenantUpdatedEvent(Guid TenantId) : DomainEventBase;
