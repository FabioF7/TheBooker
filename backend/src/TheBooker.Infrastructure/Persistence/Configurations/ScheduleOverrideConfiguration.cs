using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;

namespace TheBooker.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ScheduleOverride entity.
/// </summary>
public class ScheduleOverrideConfiguration : IEntityTypeConfiguration<ScheduleOverride>
{
    public void Configure(EntityTypeBuilder<ScheduleOverride> builder)
    {
        builder.ToTable("schedule_overrides");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.StartDate)
            .IsRequired();

        builder.Property(o => o.EndDate)
            .IsRequired();

        // Type as smart enum
        builder.Property(o => o.Type)
            .HasConversion(
                type => type.Id,
                id => OverrideType.FromId(id)!)
            .IsRequired();

        // ModifiedHours as owned type (JSONB)
        builder.OwnsOne(o => o.ModifiedHours, hours =>
        {
            hours.ToJson("modified_hours");

            hours.Property(h => h.Start);
            hours.Property(h => h.End);
        });

        builder.Property(o => o.Reason)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        // Relationships
        builder.HasOne(o => o.Tenant)
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Provider)
            .WithMany()
            .HasForeignKey(o => o.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for date-based queries
        builder.HasIndex(o => new { o.TenantId, o.StartDate, o.EndDate })
            .HasDatabaseName("ix_schedule_overrides_tenant_dates");

        builder.HasIndex(o => new { o.ProviderId, o.StartDate, o.EndDate })
            .HasDatabaseName("ix_schedule_overrides_provider_dates");

        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);
    }
}
