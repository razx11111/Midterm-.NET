using RoomHub.Api.Features.Bookings.CancelBooking;
using RoomHub.Api.Features.Bookings.CreateBooking;
using RoomHub.Api.Features.Bookings.GetBookingById;

namespace RoomHub.Api.Features.Bookings;

public static class BookingsEndpoints
{
    public static void MapBookings(this IEndpointRouteBuilder app)
    {
        var rooms = app.MapGroup("/api/rooms");
        rooms.MapCreateBooking();

        var bookings = app.MapGroup("/api/bookings").WithTags("Bookings");
        bookings.MapGetBookingById();
        bookings.MapCancelBooking();
    }
}
