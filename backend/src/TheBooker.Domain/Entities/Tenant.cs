using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.Events;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// Tenant aggregate root - represents a business using the scheduling system.
/// Multi-tenant discriminator pattern: all tenant data shares the same schema.
/// </summary>
public sealed class Tenant : AggregateRoot, IAuditableEntity
{
    private readonly List<TenantUser> _users = [];
    private readonly List<Service> _services = [];
    private readonly List<ServiceProvider> _providers = [];

    private Tenant(Guid id) : base(id) { }

    // For EF Core
    private Tenant() { }

    public Slug Slug { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string TimeZoneId { get; private set; } = "UTC";
    public BusinessHours BusinessHours { get; private set; } = null!;
    public int BufferMinutes { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation properties (read-only)
    public IReadOnlyCollection<TenantUser> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();
    public IReadOnlyCollection<ServiceProvider> Providers => _providers.AsReadOnly();

    /// <summary>
    /// Factory method to create a new Tenant with validation.
    /// </summary>
    public static Result<Tenant> Create(
        string name,
        string slug,
        string timeZoneId,
        int bufferMinutes = 0)
    {
        // Validate slug
        var slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return Error.Validation("Tenant.InvalidTimeZone", $"Invalid timezone: {timeZoneId}");
        }

        // Validate buffer
        if (bufferMinutes < 0 || bufferMinutes > 120)
            return Error.Validation("Tenant.InvalidBuffer", "Buffer must be between 0 and 120 minutes.");

        var tenant = new Tenant(Guid.NewGuid())
        {
            Name = name.Trim(),
            Slug = slugResult.Value,
            TimeZoneId = timeZoneId,
            BufferMinutes = bufferMinutes,
            BusinessHours = BusinessHours.CreateDefault(),
            CreatedAtUtc = DateTime.UtcNow
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Slug.Value));

        return tenant;
    }

    /// <summary>
    /// Updates the tenant's business hours.
    /// </summary>
    public Result UpdateBusinessHours(BusinessHours businessHours)
    {
        BusinessHours = businessHours;
        ModifiedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Updates the buffer time between appointments.
    /// </summary>
    public Result UpdateBufferMinutes(int bufferMinutes)
    {
        if (bufferMinutes < 0 || bufferMinutes > 120)
            return Error.Validation("Tenant.InvalidBuffer", "Buffer must be between 0 and 120 minutes.");

        BufferMinutes = bufferMinutes;
        ModifiedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Deactivates the tenant (soft disable).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates a deactivated tenant.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
