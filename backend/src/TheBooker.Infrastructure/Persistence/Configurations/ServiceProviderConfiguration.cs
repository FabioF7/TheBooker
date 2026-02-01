using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheBooker.Domain.Entities;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ServiceProvider entity.
/// Handles N:N relationship with Services.
/// </summary>
public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProvider>
{
    public void Configure(EntityTypeBuilder<ServiceProvider> builder)
    {
        builder.ToTable("service_providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Email as owned type (optional)
        builder.OwnsOne(p => p.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(Email.MaxLength);
        });

        // Custom BusinessHours as JSONB (optional) - store as single JSON column
        builder.OwnsOne(p => p.CustomBusinessHours, bh =>
        {
            bh.ToJson("custom_business_hours");
        });

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        // N:N relationship with Services via join table
        builder.HasMany(p => p.Services)
            .WithMany(s => s.Providers)
            .UsingEntity<Dictionary<string, object>>(
                "service_provider_services",
                j => j.HasOne<Service>().WithMany().HasForeignKey("service_id"),
                j => j.HasOne<ServiceProvider>().WithMany().HasForeignKey("provider_id"),
                j =>
                {
                    j.HasKey("provider_id", "service_id");
                    j.ToTable("service_provider_services");
                });

        // Optional link to TenantUser
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Index for tenant queries
        builder.HasIndex(p => p.TenantId);

        // Ignore domain events and navigation
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.ScheduleOverrides);
    }
}
