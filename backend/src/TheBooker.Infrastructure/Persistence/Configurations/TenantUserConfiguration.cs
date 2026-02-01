using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for TenantUser entity.
/// </summary>
public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("tenant_users");

        builder.HasKey(u => u.Id);

        // Email as owned type
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        // Role as smart enum
        builder.Property(u => u.Role)
            .HasConversion(
                role => role.Id,
                id => UserRole.FromId(id)!)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        // Composite unique index on TenantId + Email
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
