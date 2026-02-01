using TheBooker.Infrastructure.Persistence;
using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace TheBooker.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        // Get all tenants
        group.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var tenants = await db.Tenants
                .Select(t => new
                {
                    t.Id,
                    Slug = t.Slug.Value,
                    t.Name,
                    t.TimeZoneId,
                    t.BufferMinutes,
                    t.IsActive
                })
                .ToListAsync(ct);

            return Results.Ok(tenants);
        })
        .WithName("GetTenants");

        // Get tenant by slug
        group.MapGet("/{slug}", async (
            string slug,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var tenant = await db.Tenants
                .Where(t => t.Slug.Value == slug.ToLowerInvariant())
                .Select(t => new
                {
                    t.Id,
                    Slug = t.Slug.Value,
                    t.Name,
                    t.TimeZoneId,
                    t.BufferMinutes,
                    t.IsActive,
                    t.BusinessHours
                })
                .FirstOrDefaultAsync(ct);

            return tenant is null
                ? Results.NotFound()
                : Results.Ok(tenant);
        })
        .WithName("GetTenantBySlug");

        // Create tenant (for demo/seeding)
        group.MapPost("/", async (
            CreateTenantRequest request,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var tenantResult = Tenant.Create(
                request.Name,
                request.Slug,
                request.TimeZoneId,
                request.BufferMinutes);

            if (tenantResult.IsFailure)
                return Results.BadRequest(tenantResult.Error.Description);

            var tenant = tenantResult.Value;
            await db.Tenants.AddAsync(tenant, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/tenants/{tenant.Slug}", new
            {
                tenant.Id,
                Slug = tenant.Slug.Value,
                tenant.Name
            });
        })
        .WithName("CreateTenant");

        // Get services for tenant
        group.MapGet("/{tenantId:guid}/services", async (
            Guid tenantId,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var services = await db.Services
                .Where(s => s.TenantId == tenantId && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    Price = new { s.Price.Amount, s.Price.Currency }
                })
                .ToListAsync(ct);

            return Results.Ok(services);
        })
        .WithName("GetTenantServices");

        // Create service (for demo/seeding)
        group.MapPost("/{tenantId:guid}/services", async (
            Guid tenantId,
            CreateServiceRequest request,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var serviceResult = Service.Create(
                tenantId,
                request.Name,
                request.DurationMinutes,
                request.Price,
                request.Currency ?? "USD",
                request.Description);

            if (serviceResult.IsFailure)
                return Results.BadRequest(serviceResult.Error.Description);

            var service = serviceResult.Value;
            await db.Services.AddAsync(service, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/tenants/{tenantId}/services/{service.Id}", new
            {
                service.Id,
                service.Name,
                service.DurationMinutes
            });
        })
        .WithName("CreateService");

        // Get providers for tenant
        group.MapGet("/{tenantId:guid}/providers", async (
            Guid tenantId,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var providers = await db.ServiceProviders
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .Include(p => p.Services)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Email = p.Email != null ? p.Email.Value : null,
                    ServiceIds = p.Services.Select(s => s.Id).ToList()
                })
                .ToListAsync(ct);

            return Results.Ok(providers);
        })
        .WithName("GetTenantProviders");

        // Create provider (for demo/seeding)
        group.MapPost("/{tenantId:guid}/providers", async (
            Guid tenantId,
            CreateProviderRequest request,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var providerResult = Domain.Entities.ServiceProvider.Create(
                tenantId,
                request.Name,
                request.Email);

            if (providerResult.IsFailure)
                return Results.BadRequest(providerResult.Error.Description);

            var provider = providerResult.Value;

            // Assign services if provided
            if (request.ServiceIds?.Any() == true)
            {
                var services = await db.Services
                    .Where(s => request.ServiceIds.Contains(s.Id))
                    .ToListAsync(ct);

                foreach (var service in services)
                {
                    provider.AssignService(service);
                }
            }

            await db.ServiceProviders.AddAsync(provider, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/tenants/{tenantId}/providers/{provider.Id}", new
            {
                provider.Id,
                provider.Name
            });
        })
        .WithName("CreateProvider");
    }
}

// Request DTOs
public record CreateTenantRequest(
    string Name,
    string Slug,
    string TimeZoneId,
    int BufferMinutes = 0);

public record CreateServiceRequest(
    string Name,
    int DurationMinutes,
    decimal Price,
    string? Currency,
    string? Description);

public record CreateProviderRequest(
    string Name,
    string? Email,
    List<Guid>? ServiceIds);
