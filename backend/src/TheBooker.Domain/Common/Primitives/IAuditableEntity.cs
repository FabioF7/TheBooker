namespace TheBooker.Domain.Common.Primitives;

/// <summary>
/// Interface for entities that track creation and modification timestamps.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime? ModifiedAtUtc { get; }
}

/// <summary>
/// Interface for entities that support soft delete.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
}

/// <summary>
/// Interface for multi-tenant entities.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }
}
