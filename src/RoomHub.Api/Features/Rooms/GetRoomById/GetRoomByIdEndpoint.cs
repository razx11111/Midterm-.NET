using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Rooms;
using RoomHub.Api.Infrastructure.Persistence;

namespace RoomHub.Api.Features.Rooms.GetRoomById;

public sealed record GetRoomByIdResponse(Guid Id, string Name, int Capacity);

public static class GetRoomByIdEndpoint
{
    public static RouteGroupBuilder MapGetRoomById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (
            Guid id,
            RoomHubDbContext db,
            CancellationToken cancellationToken)
            : Task<Results<Ok<GetRoomByIdResponse>, ProblemHttpResult>> =>
        {
            var room = await db.Rooms
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new GetRoomByIdResponse(r.Id, r.Name, r.Capacity))
                .FirstOrDefaultAsync(cancellationToken);

            if (room is null)
                return RoomErrors.NotFound.ToProblem();

            return TypedResults.Ok(room);
        })
        .WithName("GetRoomById");

        return group;
    }
}
