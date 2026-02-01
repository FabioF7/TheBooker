using MediatR;
using TheBooker.Application.Features.Appointments.Commands;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments");

        // Hold a slot (soft lock)
        group.MapPost("/hold", async (
            HoldSlotRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new HoldSlotCommand(
                request.TenantId,
                request.ServiceId,
                request.ProviderId,
                request.Date,
                request.StartTime,
                request.SessionId);

            var result = await sender.Send(command, ct);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.Problem(
                    detail: error.Description,
                    statusCode: GetStatusCode(error)));
        })
        .WithName("HoldSlot")
        .WithDescription("Hold a time slot for 10 minutes")
        .Produces<HoldSlotResponse>(200)
        .ProducesProblem(409);

        // Confirm appointment
        group.MapPost("/{appointmentId:guid}/confirm", async (
            Guid appointmentId,
            ConfirmAppointmentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new ConfirmAppointmentCommand(
                appointmentId,
                request.SessionId,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone,
                request.Notes);

            var result = await sender.Send(command, ct);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.Problem(
                    detail: error.Description,
                    statusCode: GetStatusCode(error)));
        })
        .WithName("ConfirmAppointment")
        .WithDescription("Confirm a held appointment with customer details")
        .Produces<ConfirmAppointmentResponse>(200)
        .ProducesProblem(400);

        // Cancel appointment
        group.MapPost("/{appointmentId:guid}/cancel", async (
            Guid appointmentId,
            CancelAppointmentRequest? request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CancelAppointmentCommand(
                appointmentId,
                request?.Reason);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { message = "Appointment cancelled" })
                : Results.Problem(
                    detail: result.Error.Description,
                    statusCode: GetStatusCode(result.Error));
        })
        .WithName("CancelAppointment")
        .WithDescription("Cancel an appointment")
        .Produces(200)
        .ProducesProblem(400);
    }

    private static int GetStatusCode(Error error) => error.Code switch
    {
        var code when code.StartsWith("NotFound") => 404,
        var code when code.StartsWith("Validation") => 400,
        var code when code.StartsWith("Conflict") => 409,
        _ => 500
    };
}

// Request DTOs
public record HoldSlotRequest(
    Guid TenantId,
    Guid ServiceId,
    Guid ProviderId,
    DateOnly Date,
    TimeOnly StartTime,
    string SessionId);

public record ConfirmAppointmentRequest(
    string SessionId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? Notes);

public record CancelAppointmentRequest(
    string? Reason);
