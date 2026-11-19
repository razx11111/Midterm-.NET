using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.Bookings;

namespace RoomHub.Api.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DateRange>> GetActiveRangesForRoomAsync(
        Guid roomId, CancellationToken cancellationToken)
    {
        return await db.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .Select(b => b.Range)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveBookingsForParticipantOnDayAsync(
        string bookedBy, DateOnly dayUtc, CancellationToken cancellationToken)
    {
        var dayStart = dayUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var nextDay = dayStart.AddDays(1);

        return db.Bookings
            .AsNoTracking()
            .Where(b => b.BookedBy == bookedBy
                && b.Status == BookingStatus.Active
                && b.Range.StartUtc >= dayStart
                && b.Range.StartUtc < nextDay)
            .CountAsync(cancellationToken);
    }

    public void Add(Booking booking) => db.Bookings.Add(booking);
}
