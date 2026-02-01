using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// User within a tenant - handles authentication and authorization.
/// </summary>
public sealed class TenantUser : Entity, IAuditableEntity, ITenantEntity
{
    private TenantUser(Guid id) : base(id) { }

    // For EF Core
    private TenantUser() { }

    public Guid TenantId { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAtUtc { get; private set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;

    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Factory method to create a new TenantUser.
    /// </summary>
    public static Result<TenantUser> Create(
        Guid tenantId,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("TenantUser.FirstNameRequired", "First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("TenantUser.LastNameRequired", "Last name is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Error.Validation("TenantUser.PasswordRequired", "Password is required.");

        return new TenantUser(Guid.NewGuid())
        {
            TenantId = tenantId,
            Email = emailResult.Value,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the password hash.
    /// </summary>
    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's role.
    /// </summary>
    public void UpdateRole(UserRole role)
    {
        Role = role;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
