using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

public sealed class Booking
{
    private const int DailyActiveLimit = 2;
    private const int LargeRoomCapacityThreshold = 10;
    private static readonly TimeSpan CancellationLeadTime = TimeSpan.FromMinutes(60);

    private Booking()
    {
    }

    private Booking(Guid id, Guid roomId, string bookedBy, DateRange range, int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        Range = range;
        Attendees = attendees;
        Status = BookingStatus.Active;
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateRange Range { get; private set; } = null!;
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    public static Result<Booking> Create(
        Guid roomId,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        int roomCapacity,
        DateTime nowUtc,
        IEnumerable<DateRange> existingActiveRanges,
        int sameDayActiveCount)
    {
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.BookedByRequired;

        var rangeResult = DateRange.Create(startUtc, endUtc);
        if (rangeResult.IsFailure)
            return rangeResult.Error;

        var range = rangeResult.Value;

        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;

        if (attendees > roomCapacity)
            return BookingErrors.AttendeesExceedCapacity;

        if (range.StartUtc < nowUtc)
            return BookingErrors.StartInPast;

        var maxDuration = roomCapacity > LargeRoomCapacityThreshold
            ? TimeSpan.FromHours(2)
            : TimeSpan.FromHours(4);

        if (range.EndUtc - range.StartUtc > maxDuration)
            return BookingErrors.DurationExceeded;

        if (existingActiveRanges.Any(existing => existing.Overlaps(range)))
            return BookingErrors.RoomOccupied;

        if (sameDayActiveCount >= DailyActiveLimit)
            return BookingErrors.DailyLimitReached;

        return new Booking(Guid.NewGuid(), roomId, bookedBy.Trim(), range, attendees);
    }

    public Result<Unit> Cancel(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        if (Range.StartUtc - nowUtc < CancellationLeadTime)
            return BookingErrors.CancellationTooLate;

        Status = BookingStatus.Cancelled;
        return Unit.Value;
    }
}
