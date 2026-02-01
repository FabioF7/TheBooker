using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Appointment entity.
/// Includes strategic indexes for availability queries and cleanup jobs.
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.DurationMinutes)
            .IsRequired();

        // Status as smart enum
        builder.Property(a => a.Status)
            .HasConversion(
                status => status.Id,
                id => AppointmentStatus.FromId(id)!)
            .IsRequired();

        // Soft lock fields
        builder.Property(a => a.LockedAt);
        builder.Property(a => a.ExpiresAt);
        builder.Property(a => a.SessionId)
            .HasMaxLength(100);

        // CustomerInfo as owned type (JSONB)
        builder.OwnsOne(a => a.Customer, customer =>
        {
            customer.ToJson("customer_info");

            customer.Property(c => c.Name).IsRequired();
            customer.Property(c => c.Phone);
            customer.Property(c => c.Notes);

            customer.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value).IsRequired();
            });
        });

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        // Relationships
        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Provider)
            .WithMany()
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Strategic Indexes
        // Index for availability queries: TenantId, Date, Status
        builder.HasIndex(a => new { a.TenantId, a.Date, a.Status })
            .HasDatabaseName("ix_appointments_tenant_date_status");

        // Index for provider availability queries
        builder.HasIndex(a => new { a.ProviderId, a.Date, a.Status })
            .HasDatabaseName("ix_appointments_provider_date_status");

        // Index for cleanup jobs: ExpiresAt
        builder.HasIndex(a => a.ExpiresAt)
            .HasDatabaseName("ix_appointments_expires_at")
            .HasFilter("\"ExpiresAt\" IS NOT NULL");

        // Index for soft lock validation
        builder.HasIndex(a => a.SessionId)
            .HasDatabaseName("ix_appointments_session_id")
            .HasFilter("\"SessionId\" IS NOT NULL");

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}
