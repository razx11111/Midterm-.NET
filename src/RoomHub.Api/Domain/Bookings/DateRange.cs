using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

public sealed record DateRange
{
    private DateRange()
    {
    }

    private DateRange(DateTime startUtc, DateTime endUtc)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }

    public static Result<DateRange> Create(DateTime start, DateTime end)
    {
        var startUtc = Normalize(start);
        var endUtc = Normalize(end);

        if (endUtc <= startUtc)
            return DateRangeErrors.EndBeforeStart;

        return new DateRange(startUtc, endUtc);
    }

    public bool Overlaps(DateRange other) =>
        StartUtc < other.EndUtc && other.StartUtc < EndUtc;

    private static DateTime Normalize(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
