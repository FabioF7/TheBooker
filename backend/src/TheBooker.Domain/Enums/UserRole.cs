using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Domain.Enums;

/// <summary>
/// Smart enumeration for user roles within a tenant.
/// </summary>
public sealed class UserRole : Enumeration<UserRole>
{
    public static readonly UserRole Admin = new(1, nameof(Admin));
    public static readonly UserRole Provider = new(2, nameof(Provider));
    public static readonly UserRole Staff = new(3, nameof(Staff));
    public static readonly UserRole Customer = new(4, nameof(Customer));

    private UserRole(int id, string name) : base(id, name) { }

    /// <summary>
    /// Determines if this role can manage other users.
    /// </summary>
    public bool CanManageUsers => this == Admin;

    /// <summary>
    /// Determines if this role can manage appointments.
    /// </summary>
    public bool CanManageAppointments => this == Admin || this == Staff || this == Provider;

    /// <summary>
    /// Determines if this role can view all appointments in tenant.
    /// </summary>
    public bool CanViewAllAppointments => this == Admin || this == Staff;

    /// <summary>
    /// Determines if this role can provide services.
    /// </summary>
    public bool CanProvideServices => this == Provider;
}
