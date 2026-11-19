using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid RoomId,
    string? BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees) : ICommand<Booking>;

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : ICommandHandler<CreateBookingCommand, Booking>
{
    public async Task<Result<Booking>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var room = await rooms.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null)
            return RoomErrors.NotFound;

        var activeRanges = await bookings.GetActiveRangesForRoomAsync(command.RoomId, cancellationToken);
        var nowUtc = time.GetUtcNow().UtcDateTime;

        var rangeResult = DateRange.Create(command.StartUtc, command.EndUtc);
        var dayUtc = rangeResult.IsSuccess
            ? DateOnly.FromDateTime(rangeResult.Value.StartUtc)
            : DateOnly.FromDateTime(nowUtc);

        var sameDayActiveCount = command.BookedBy is { } bookedBy && !string.IsNullOrWhiteSpace(bookedBy)
            ? await bookings.CountActiveBookingsForParticipantOnDayAsync(bookedBy.Trim(), dayUtc, cancellationToken)
            : 0;

        var result = Booking.Create(
            command.RoomId,
            command.BookedBy,
            command.StartUtc,
            command.EndUtc,
            command.Attendees,
            room.Capacity,
            nowUtc,
            activeRanges,
            sameDayActiveCount);

        if (result.IsFailure)
            return result.Error;

        var booking = result.Value;
        bookings.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return booking;
    }
}
