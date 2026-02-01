namespace TheBooker.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "TheBooker API",
            version = "1.0.0"
        }))
        .WithName("HealthCheck")
        .WithTags("Health");
    }
}
