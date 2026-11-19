namespace RoomHub.Api.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<DateRange>> GetActiveRangesForRoomAsync(
        Guid roomId, CancellationToken cancellationToken);

    Task<int> CountActiveBookingsForParticipantOnDayAsync(
        string bookedBy, DateOnly dayUtc, CancellationToken cancellationToken);

    void Add(Booking booking);
}
