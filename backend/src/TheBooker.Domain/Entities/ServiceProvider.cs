using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// Service provider (staff member who provides services).
/// Has N:N relationship with Services.
/// </summary>
public sealed class ServiceProvider : Entity, IAuditableEntity, ITenantEntity
{
    private readonly List<Service> _services = [];
    private readonly List<ScheduleOverride> _scheduleOverrides = [];

    private ServiceProvider(Guid id) : base(id) { }

    // For EF Core
    private ServiceProvider() { }

    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; } // Optional link to TenantUser
    public string Name { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public BusinessHours? CustomBusinessHours { get; private set; } // Override tenant hours
    public bool IsActive { get; private set; } = true;

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;
    public TenantUser? User { get; private set; }
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();
    public IReadOnlyCollection<ScheduleOverride> ScheduleOverrides => _scheduleOverrides.AsReadOnly();

    /// <summary>
    /// Factory method to create a new ServiceProvider.
    /// </summary>
    public static Result<ServiceProvider> Create(
        Guid tenantId,
        string name,
        string? email = null,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("ServiceProvider.NameRequired", "Provider name is required.");

        Email? emailVo = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return emailResult.Error;
            emailVo = emailResult.Value;
        }

        return new ServiceProvider(Guid.NewGuid())
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = emailVo,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sets custom business hours for this provider (overrides tenant default).
    /// </summary>
    public Result SetCustomBusinessHours(BusinessHours hours)
    {
        CustomBusinessHours = hours;
        ModifiedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Clears custom business hours (revert to tenant default).
    /// </summary>
    public void ClearCustomBusinessHours()
    {
        CustomBusinessHours = null;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the effective business hours (custom or tenant default).
    /// </summary>
    public BusinessHours GetEffectiveBusinessHours(Tenant tenant)
    {
        return CustomBusinessHours ?? tenant.BusinessHours;
    }

    /// <summary>
    /// Assigns a service to this provider.
    /// </summary>
    public void AssignService(Service service)
    {
        if (!_services.Contains(service))
        {
            _services.Add(service);
            ModifiedAtUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a service from this provider.
    /// </summary>
    public void RemoveService(Service service)
    {
        _services.Remove(service);
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
