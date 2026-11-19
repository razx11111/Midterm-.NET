using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public static class CancelBookingEndpoint
{
    public static RouteGroupBuilder MapCancelBooking(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/cancel", async (
            Guid id,
            ICommandHandler<CancelBookingCommand, Unit> handler,
            CancellationToken cancellationToken)
            : Task<Results<NoContent, ProblemHttpResult>> =>
        {
            var result = await handler.Handle(new CancelBookingCommand(id), cancellationToken);

            if (result.IsFailure)
                return result.Error.ToProblem();

            return TypedResults.NoContent();
        })
        .WithName("CancelBooking");

        return group;
    }
}
