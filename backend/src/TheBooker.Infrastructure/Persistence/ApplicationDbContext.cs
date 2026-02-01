using Microsoft.EntityFrameworkCore;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Common.Primitives;
using System.Reflection;

namespace TheBooker.Infrastructure.Persistence;

/// <summary>
/// Application DbContext with PostgreSQL configuration.
/// Handles domain events dispatch and multi-tenant filtering.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ScheduleOverride> ScheduleOverrides => Set<ScheduleOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves changes and dispatches domain events.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all domain events before saving
        var domainEvents = ChangeTracker.Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear domain events from entities
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Dispatch domain events via MediatR
        // This would typically be handled by a separate service

        return result;
    }
}
