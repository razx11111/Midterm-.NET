using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

public static class DateRangeErrors
{
    public static readonly Error EndBeforeStart = Error.Validation(
        "DateRange.EndBeforeStart",
        "The end must be strictly after the start.");
}
