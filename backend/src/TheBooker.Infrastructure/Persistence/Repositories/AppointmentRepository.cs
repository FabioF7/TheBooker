using Microsoft.EntityFrameworkCore;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;

namespace TheBooker.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Appointment aggregate.
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.ProviderId == providerId && a.Date == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetActiveByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var pendingId = AppointmentStatus.Pending.Id;
        var confirmedId = AppointmentStatus.Confirmed.Id;
        var now = DateTime.UtcNow;

        return await _context.Appointments
            .Where(a => a.ProviderId == providerId &&
                        a.Date == date &&
                        (a.Status == AppointmentStatus.Confirmed ||
                         (a.Status == AppointmentStatus.Pending && a.ExpiresAt > now)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetExpiredPendingAppointmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Pending &&
                        a.ExpiresAt != null &&
                        a.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConflictAsync(
        Guid providerId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _context.Appointments
            .Where(a => a.ProviderId == providerId &&
                        a.Date == date &&
                        (a.Status == AppointmentStatus.Confirmed ||
                         (a.Status == AppointmentStatus.Pending && a.ExpiresAt > now)) &&
                        a.StartTime < endTime &&
                        a.EndTime > startTime);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(entity, cancellationToken);
    }

    public void Update(Appointment entity)
    {
        _context.Appointments.Update(entity);
    }

    public void Remove(Appointment entity)
    {
        _context.Appointments.Remove(entity);
    }
}
