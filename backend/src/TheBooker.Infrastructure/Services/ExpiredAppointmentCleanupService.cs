using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheBooker.Application.Common.Interfaces;

namespace TheBooker.Infrastructure.Services;

/// <summary>
/// Background service that cleans up expired pending appointments.
/// Runs every minute to release soft-locked slots.
/// </summary>
public sealed class ExpiredAppointmentCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredAppointmentCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public ExpiredAppointmentCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredAppointmentCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Appointment Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredAppointmentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired appointment cleanup");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Expired Appointment Cleanup Service stopped");
    }

    private async Task CleanupExpiredAppointmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointmentRepository = scope.ServiceProvider
            .GetRequiredService<IAppointmentRepository>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var expiredAppointments = await appointmentRepository
            .GetExpiredPendingAppointmentsAsync(cancellationToken);

        if (expiredAppointments.Count == 0)
            return;

        _logger.LogInformation(
            "Found {Count} expired pending appointments to cleanup",
            expiredAppointments.Count);

        foreach (var appointment in expiredAppointments)
        {
            // Cancel the expired appointment
            var result = appointment.Cancel("Expired - slot lock timeout");

            if (result.IsSuccess)
            {
                appointmentRepository.Update(appointment);
                _logger.LogDebug(
                    "Cancelled expired appointment {AppointmentId}",
                    appointment.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cleaned up {Count} expired appointments",
            expiredAppointments.Count);
    }
}
