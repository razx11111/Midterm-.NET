using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error BookedByRequired = Error.Validation(
        "Booking.BookedByRequired",
        "BookedBy is required.");

    public static readonly Error InvalidAttendees = Error.Validation(
        "Booking.InvalidAttendees",
        "Attendees must be strictly positive.");

    public static readonly Error AttendeesExceedCapacity = Error.Validation(
        "Booking.AttendeesExceedCapacity",
        "The room does not have enough seats.");

    public static readonly Error StartInPast = Error.Validation(
        "Booking.StartInPast",
        "A booking cannot start in the past.");

    public static readonly Error DurationExceeded = Error.Validation(
        "Booking.DurationExceeded",
        "The booking exceeds the maximum allowed duration.");

    public static readonly Error RoomOccupied = Error.Conflict(
        "Booking.RoomOccupied",
        "The room is occupied in this interval.");

    public static readonly Error DailyLimitReached = Error.Conflict(
        "Booking.DailyLimitReached",
        "The participant already has the maximum active bookings for this day.");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "Booking.AlreadyCancelled",
        "The booking is already cancelled.");

    public static readonly Error CancellationTooLate = Error.Conflict(
        "Booking.CancellationTooLate",
        "A booking can only be cancelled at least 60 minutes before it starts.");

    public static readonly Error NotFound = Error.NotFound(
        "Booking.NotFound",
        "The booking does not exist.");
}
