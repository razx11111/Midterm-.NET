using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Features.Rooms.CreateRoom;

public sealed record CreateRoomCommand(string? Name, int Capacity) : ICommand<Room>;

public sealed class CreateRoomHandler(IRoomRepository rooms, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoomCommand, Room>
{
    public async Task<Result<Room>> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var result = Room.Create(command.Name, command.Capacity);
        if (result.IsFailure)
            return result.Error;

        var room = result.Value;
        rooms.Add(room);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return room;
    }
}
