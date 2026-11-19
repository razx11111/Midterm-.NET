using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;
using RoomHub.Api.Features.Bookings;
using RoomHub.Api.Features.Rooms;
using RoomHub.Api.Infrastructure.Persistence;
using RoomHub.Api.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<RoomHubDbContext>((serviceProvider, options) =>
    options.UseSqlite(
        serviceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("RoomHub")));

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RoomHubDbContext>());
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHandlers(typeof(Program).Assembly);

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.MapGet("/", () => "RoomHub API is running.");
app.MapRooms();
app.MapBookings();

app.Run();
