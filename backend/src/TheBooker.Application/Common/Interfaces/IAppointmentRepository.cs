using TheBooker.Domain.Entities;

namespace TheBooker.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Appointment aggregate.
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    /// <summary>
    /// Gets appointments for a provider on a specific date.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (Pending/Confirmed) appointments for a provider on a date.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetActiveByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired pending appointments for cleanup.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetExpiredPendingAppointmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a time slot conflicts with existing appointments.
    /// </summary>
    Task<bool> HasConflictAsync(
        Guid providerId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default);
}
