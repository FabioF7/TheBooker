using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// Service offered by a tenant (e.g., "Haircut", "Consultation").
/// </summary>
public sealed class Service : Entity, IAuditableEntity, ITenantEntity
{
    private readonly List<ServiceProvider> _providers = [];

    private Service(Guid id) : base(id) { }

    // For EF Core
    private Service() { }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public Money Price { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;
    public IReadOnlyCollection<ServiceProvider> Providers => _providers.AsReadOnly();

    /// <summary>
    /// Factory method to create a new Service.
    /// </summary>
    public static Result<Service> Create(
        Guid tenantId,
        string name,
        int durationMinutes,
        decimal price,
        string currency = "USD",
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Service.NameRequired", "Service name is required.");

        if (name.Length > 100)
            return Error.Validation("Service.NameTooLong", "Service name cannot exceed 100 characters.");

        if (durationMinutes < 5 || durationMinutes > 480)
            return DomainErrors.Service.InvalidDuration;

        var priceResult = Money.Create(price, currency);
        if (priceResult.IsFailure)
            return priceResult.Error;

        return new Service(Guid.NewGuid())
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DurationMinutes = durationMinutes,
            Price = priceResult.Value,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates service details.
    /// </summary>
    public Result Update(string name, int durationMinutes, decimal price, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Service.NameRequired", "Service name is required.");

        if (durationMinutes < 5 || durationMinutes > 480)
            return DomainErrors.Service.InvalidDuration;

        var priceResult = Money.Create(price, Price.Currency);
        if (priceResult.IsFailure)
            return priceResult.Error;

        Name = name.Trim();
        DurationMinutes = durationMinutes;
        Price = priceResult.Value;
        Description = description?.Trim();
        ModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
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
