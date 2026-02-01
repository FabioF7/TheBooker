using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheBooker.Application.Features.Availability;
using TheBooker.Application.Services.Availability;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Api.Endpoints;

public static class AvailabilityEndpoints
{
    public static void MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/availability")
            .WithTags("Availability");

        group.MapGet("/{tenantId:guid}/{providerId:guid}/{serviceId:guid}/{date}",
            async (
                Guid tenantId,
                Guid providerId,
                Guid serviceId,
                DateOnly date,
                [FromQuery] int slotInterval,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAvailabilityQuery(
                    tenantId,
                    providerId,
                    serviceId,
                    date,
                    slotInterval > 0 ? slotInterval : 15);

                var result = await sender.Send(query, ct);

                return result.Match(
                    onSuccess: availability => Results.Ok(availability),
                    onFailure: error => Results.Problem(
                        detail: error.Description,
                        statusCode: GetStatusCode(error)));
            })
            .WithName("GetAvailability")
            .WithDescription("Get available time slots for a provider on a specific date")
            .Produces<Services.Availability.AvailabilityResponse>(200)
            .ProducesProblem(404);
    }

    private static int GetStatusCode(Error error) => error.Code switch
    {
        var code when code.StartsWith("NotFound") => 404,
        var code when code.StartsWith("Validation") => 400,
        var code when code.StartsWith("Conflict") => 409,
        _ => 500
    };
}
