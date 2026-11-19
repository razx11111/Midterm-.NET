using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Infrastructure.Persistence;

namespace RoomHub.Api.Features.Bookings.GetBookingById;

public sealed record GetBookingByIdResponse(
    Guid Id,
    Guid RoomId,
    string BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees,
    string Status);

public static class GetBookingByIdEndpoint
{
    public static RouteGroupBuilder MapGetBookingById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (
            Guid id,
            RoomHubDbContext db,
            CancellationToken cancellationToken)
            : Task<Results<Ok<GetBookingByIdResponse>, ProblemHttpResult>> =>
        {
            var booking = await db.Bookings
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new GetBookingByIdResponse(
                    b.Id,
                    b.RoomId,
                    b.BookedBy,
                    b.Range.StartUtc,
                    b.Range.EndUtc,
                    b.Attendees,
                    b.Status.ToString()))
                .FirstOrDefaultAsync(cancellationToken);

            if (booking is null)
                return BookingErrors.NotFound.ToProblem();

            return TypedResults.Ok(booking);
        })
        .WithName("GetBookingById");

        return group;
    }
}
