using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheBooker.Domain.Entities;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Tenant entity.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        // Slug as owned type with unique index
        builder.OwnsOne(t => t.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(Slug.MaxLength)
                .IsRequired();

            slug.HasIndex(s => s.Value)
                .IsUnique();
        });

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.TimeZoneId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.BufferMinutes)
            .HasDefaultValue(0);

        // JSONB mapping for BusinessHours - store as single JSON column
        builder.OwnsOne(t => t.BusinessHours, bh =>
        {
            bh.ToJson("business_hours");
        });

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        // Relationships
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Services)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Providers)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}
