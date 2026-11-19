using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingRequest(
    string? BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees);

public sealed record CreateBookingResponse(Guid Id, Guid RoomId, string Status);

public static class CreateBookingEndpoint
{
    public static RouteGroupBuilder MapCreateBooking(this RouteGroupBuilder group)
    {
        group.MapPost("/{roomId:guid}/bookings", async Task<Results<Created<CreateBookingResponse>, ProblemHttpResult>> (
            Guid roomId,
            CreateBookingRequest request,
            ICommandHandler<CreateBookingCommand, Booking> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new CreateBookingCommand(
                    roomId, request.BookedBy, request.StartUtc, request.EndUtc, request.Attendees),
                cancellationToken);

            if (result.IsFailure)
                return result.Error.ToProblem();

            var booking = result.Value;
            return TypedResults.Created(
                $"/api/bookings/{booking.Id}",
                new CreateBookingResponse(booking.Id, booking.RoomId, booking.Status.ToString()));
        })
        .WithName("CreateBooking")
        .WithTags("Bookings");

        return group;
    }
}
