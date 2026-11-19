---
name: midterm-checker
description: >
  Implement, debug, review, or complete the RoomHub .NET 10 midterm
  repository according to ASSIGNMENT.md and CampusHub lectures 01-06.
  Refactor layered Minimal APIs into vertical slices with a rich domain,
  Result<T>, Error, custom CQRS command handlers, EF Core SQLite,
  domain repository contracts, and DbContext-based Unit of Work.
  Preserve the exact HTTP contract and verify business rules incrementally.
---

# RoomHub .NET Midterm

## 1. Purpose and source of truth

Refactor the existing RoomHub API into the architecture demonstrated in
CampusHub lectures 01-06 while preserving the assignment's HTTP contract.

Expected repository:
`https://github.com/razx11111/Midterm-.NET`

Do not assume the repository has already been inspected or that its current
contents match a previously described starter.

### Specification precedence

Use this order:

1. The actual `ASSIGNMENT.md` and explicit assignment amendments.
2. Explicit user clarifications consistent with the assignment.
3. Supplied CampusHub lecture materials and available course source code.
4. This skill's RoomHub baseline and implementation defaults.
5. Starter behavior.

The starter may contain intentional bugs. Do not preserve behavior that
contradicts the specification.

If sources conflict:
- Follow the higher-priority source.
- State the discrepancy briefly.
- Ask for clarification when the conflict materially affects correctness
  and cannot be resolved from the available materials.

Never claim to have inspected unavailable files, read unavailable lecture
source code, executed commands, or verified untested behavior.

---

## 2. Course alignment

Apply the progression taught in the supplied lectures:

| Lecture | Relevant implementation |
|---------|-------------------------|
| 01 - OOP | Encapsulation, interfaces, records, value equality, composition, half-open intervals |
| 02 - Minimal API | DI, TypedResults, route groups, `{id:guid}` constraints, `.WithName(...)`, one folder per feature, slice-owned DTOs |
| 03 - Rich Domain and Result | Private constructors/setters, factories, `Result<T>`, `Error`, `DateRange`, `TimeProvider` |
| 04 - EF Core + SQLite | `DbContext`, configurations, `ComplexProperty`, `UtcDateTimeConverter`, database initialization, `AsNoTracking` |
| 05 - Repository and Unit of Work | Domain contracts, Infrastructure implementations, shared scoped `DbContext`, no separate `UnitOfWork` class |
| 06 - CQRS commands | `ICommand`, `ICommandHandler`, orchestration handlers, thin endpoints, automatic scoped registration (`AddHandlers`) |

### Architectural boundaries

- Domain owns business invariants and state transitions.
- Handlers load data, invoke domain behavior, and save successful changes.
- Write-side persistence queries belong in repository implementations.
- Read endpoints may use `DbContext` directly with `AsNoTracking()`.
- Endpoints translate HTTP input/output; they do not implement business rules.
- Domain must not reference EF Core or ASP.NET Core.

Do not introduce:
- MediatR.
- AutoMapper.
- A generic repository.
- A separate `UnitOfWork` implementation class.
- An unnecessary service layer or query mediator.
- Extra projects, event buses, domain events, or other architecture not
  required by the assignment.

The lectures demonstrate these patterns for this course; do not present
them as mandatory architecture for every .NET application.

---

## 3. Audit before editing

Read and inspect:

1. `ASSIGNMENT.md` completely.
2. Repository status and existing project structure.
3. `Program.cs`, the `.csproj`, `appsettings.json`, and launch settings.
4. Existing endpoints, services, repositories, models, and error handling.
5. `roomhub.http` and any automated tests.
6. `BUGS.md`, `DECISIONS.md`, and `AI-LOG.md`, including authorship instructions.
7. Available course source files when a slide excerpt leaves a detail unclear.

Establish a baseline:
- Run `dotnet build`.
- Run existing tests if present.
- Note existing failures separately from introduced failures.
- Identify the current API contract and specification mismatches.

Before implementation, form a short plan containing:
- Required routes and response shapes.
- Business rules and boundary cases.
- Architectural changes.
- Verification steps.
- Any unresolved requirement.

Preserve unrelated user changes. Do not perform destructive Git operations
or delete existing data without authorization.

---

## 4. RoomHub HTTP contract

Verify this baseline against `ASSIGNMENT.md` before using it.

| Operation | Route | Success | Location |
|-----------|-------|---------|----------|
| Create room | `POST /api/rooms` | 201 | `/api/rooms/{id}` |
| Get room | `GET /api/rooms/{id:guid}` | 200 | - |
| Create booking | `POST /api/rooms/{roomId:guid}/bookings` | 201 | `/api/bookings/{id}` |
| Get booking | `GET /api/bookings/{id:guid}` | 200 | - |
| Cancel booking | `POST /api/bookings/{id:guid}/cancel` | 204, no body | - |

### Request and response fields

**Create room request**
- `name`
- `capacity`

**Create/get room response**
- `id`
- `name`
- `capacity`

**Create booking request**
- `bookedBy`
- `startUtc`
- `endUtc`
- `attendees`

**Create booking response**
- `id`
- `roomId`
- `status`

**Get booking response**
- `id`
- `roomId`
- `bookedBy`
- `startUtc`
- `endUtc`
- `attendees`
- `status`

Preserve:
- Methods, routes, and route constraints (`{id:guid}`, `{roomId:guid}`).
- Endpoint metadata names via `.WithName(...)` (e.g., `"CreateRoom"`, `"GetRoomById"`, `"CreateBooking"`, `"GetBookingById"`, `"CancelBooking"`).
- Request/response property names and JSON casing.
- Required status codes and error bodies.
- Location headers (note that `POST /api/rooms/{roomId:guid}/bookings` returns `/api/bookings/{id}`).
- Timestamp semantics.
- String enum representation: `"Active"` and `"Cancelled"`.

Do not serialize domain entities directly when that would expose a
different response shape, such as a nested `range`.

Each slice owns its `Request` and `Response` types, following Lecture 02.
Shared domain primitives are not HTTP DTOs and may be reused.

Do not copy fixed sample dates into time-sensitive tests. Generate future
dates relative to a controlled clock or the test execution time.

---

## 5. Target organization

Keep the existing single ASP.NET Core project unless the assignment says
otherwise.

Suggested structure; exact filenames are not mandatory unless specified:

```text
Common/
  Cqrs/
    ICommand.cs
    ICommandHandler.cs
    HandlerRegistrationExtensions.cs
  ErrorHttpExtensions.cs

Domain/
  Common/
    Error.cs
    Result.cs
    Unit.cs
    IUnitOfWork.cs
  Rooms/
    Room.cs
    RoomErrors.cs
    IRoomRepository.cs
  Bookings/
    Booking.cs
    BookingStatus.cs
    BookingErrors.cs
    DateRange.cs
    DateRangeErrors.cs
    IBookingRepository.cs

Features/
  Rooms/
    CreateRoom/
    GetRoomById/
    RoomsEndpoints.cs
  Bookings/
    CreateBooking/
    GetBookingById/
    CancelBooking/
    BookingsEndpoints.cs

Infrastructure/
  Persistence/
    RoomHubDbContext.cs
    DatabaseInitializer.cs
    Configurations/
    Repositories/
    UtcDateTimeConverter.cs
```

Required feature slices:
1. `CreateRoom`
2. `GetRoomById`
3. `CreateBooking`
4. `GetBookingById`
5. `CancelBooking`

Write slices contain their command, handler, and endpoint. A command may
share a file with its handler, as in the course, or use its own file.

Note on route group mounting:
- `RoomsEndpoints.MapRooms(app)` maps `/api/rooms` (`CreateRoomEndpoint`, `GetRoomByIdEndpoint`).
- `BookingsEndpoints.MapBookings(app)` can either map `/api/bookings` (`GetBookingByIdEndpoint`, `CancelBookingEndpoint`) and `/api/rooms/{roomId:guid}/bookings` (`CreateBookingEndpoint`), or `CreateBookingEndpoint` can be mounted cleanly under the `/api/rooms` group with `.WithTags("Bookings")`. Choose a clean, readable layout that preserves the exact routes.

Remove obsolete services, in-memory implementations, and business-exception
handling only after their replacements are wired and working. Preserve
unrelated technical exception handling.

---

## 6. Domain primitives

### Error

Implement:

```csharp
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type)
{
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);
}
```

Define reusable errors in `RoomErrors`, `BookingErrors`, and
`DateRangeErrors`. Codes must remain stable.

### Result<T>

Follow Lecture 03:
- Private constructor.
- `Success` and `Failure` factories.
- `IsSuccess` and `IsFailure`.
- `Value` and `Error`.
- Implicit conversion from `T`.
- Implicit conversion from `Error`.

Reading `Value` on failure or `Error` on success may throw
`InvalidOperationException`: this signals programmer misuse, not a
business failure.

Expected validation, missing resources, and conflicts return `Error` values.
Unexpected technical failures may still use exceptions.

Implement `Unit` with `Unit.Value` for successful operations without a
meaningful result.

### DateRange

Use a sealed record to retain the value equality taught in the course.

Requirements:
- `StartUtc` and `EndUtc` with `private set`.
- Private parameterless constructor (required for EF Core `ComplexProperty` materialization) and a private initializing constructor `(DateTime startUtc, DateTime endUtc)`.
- Creation through `Result<DateRange> Create(DateTime start, DateTime end)`.
- Normalize both inputs before comparing them.
- Reject `EndUtc <= StartUtc`.

RoomHub normalization default, unless the actual specification or course
source requires otherwise:
- `DateTimeKind.Utc`: unchanged.
- `DateTimeKind.Local`: `ToUniversalTime()`.
- `DateTimeKind.Unspecified`: interpret as UTC using `DateTime.SpecifyKind(value, DateTimeKind.Utc)`.

Do not rely on the host timezone for `Unspecified` timestamps.

Overlap uses half-open intervals:

```csharp
public bool Overlaps(DateRange other) =>
    StartUtc < other.EndUtc &&
    other.StartUtc < EndUtc;
```

Touching intervals do not overlap. Do not use `<=`.

---

## 7. Rich domain and business rules

### Room

Implement a sealed entity with:
- Private parameterless constructor.
- Private setters.
- Domain-generated `Guid`.
- Static `Create(string? name, int capacity)` returning `Result<Room>`.

Rules:
- Name is required (`!string.IsNullOrWhiteSpace(name)`).
- Store the trimmed name (`name.Trim()`).
- Capacity must be positive (`capacity > 0`).

Suggested errors:
- `Room.NameRequired`
- `Room.InvalidCapacity`
- `Room.NotFound`

Do not invent additional rejection rules, such as arbitrary maximum
lengths, unless required by the assignment or established course model.

### Booking

Implement a sealed entity with:
- Private parameterless constructor.
- Private setters.
- Domain-generated `Guid`.
- `RoomId`, `BookedBy`, `Range`, `Attendees`, and `Status`.
- Initial status `BookingStatus.Active`.

Define:

```csharp
public enum BookingStatus
{
    Active,
    Cancelled
}
```

The factory receives the information needed to enforce the rules:
- Room id (`Guid roomId`).
- BookedBy (`string? bookedBy`, trimmed on store).
- Start and end (`DateTime startUtc`, `DateTime endUtc`).
- Attendees (`int attendees`).
- Room capacity (`int roomCapacity`).
- Current UTC time (`DateTime nowUtc`).
- Existing active booking intervals (`IEnumerable<DateRange> existingActiveRanges`).
- Count of the participant's existing active bookings on the same UTC day (`int sameDayActiveCount` for this `bookedBy` and the UTC date of `startUtc`), needed for R9.

Do not inject repositories, `DbContext`, or `TimeProvider` into the entity.

### Creation precedence

For a bindable request, the handler must check room existence before
invoking booking validation. A missing room returns 404 even when booking
details are also invalid.

This does not override framework rejection of malformed JSON or invalid
parameter binding before the handler executes.

Unless `ASSIGNMENT.md` specifies another order, validate in this order:

1. `BookedBy` is not null, empty, or whitespace.
2. `DateRange.Create` succeeds.
3. `Attendees` is positive (`> 0`).
4. `Attendees` does not exceed room capacity (`<= roomCapacity`).
5. Start is not in the past.
6. Duration does not exceed the room's maximum (R6/R10, see below).
7. No existing active interval overlaps.
8. The participant has not already reached the per-day active-booking limit (R9).

Use normalized range values for time and duration comparisons:

```csharp
range.StartUtc < nowUtc

var maxDuration = roomCapacity > 10
    ? TimeSpan.FromHours(2)   // R10
    : TimeSpan.FromHours(4);  // R6
range.EndUtc - range.StartUtc > maxDuration
```

Therefore:
- Start exactly equal to supplied `nowUtc` is valid.
- For capacity <= 10, exactly four hours is valid; for capacity > 10, exactly two hours is valid.
- Adjacent bookings are valid.
- Cancelled bookings do not block an interval.

The repository supplies active intervals; the domain evaluates overlap.

### R9 - per-participant daily active-booking limit

Exact rule from `ASSIGNMENT.md` (verify the current wording before relying on it):

> A participant cannot have more than 2 active bookings per day, across all
> rooms together. The "day" is the UTC date of `startUtc`. The third active
> booking on the same day, for the same `bookedBy`, is rejected with 409.
> Cancelled bookings do not count. Bookings on different days do not affect
> each other. The rule must live in the domain/handler, not in the endpoint.
> You may extend the repository with a new method.

Implementation:
- The limit is 2; a request that would become the third active booking is rejected.
- Reject when `sameDayActiveCount >= 2`.
- "Day" is the UTC calendar date of the normalized `startUtc` (`range.StartUtc.Date`), not a rolling 24-hour window.
- Match the participant by the same `bookedBy` value used for storage (trimmed); confirm whether the comparison is case-sensitive from the assignment or course source and state the assumption if unspecified.
- The count spans all rooms, so it is independent of the per-room overlap check (R7).
- Only `Active` bookings count; `Cancelled` ones do not.
- This is a `Conflict` error (409), like R7 and repeated cancellation.

The rule lives in the domain factory (or the handler orchestration), not the
endpoint. The domain cannot derive the count from `existingActiveRanges`
alone, because that set is per-room and carries no `bookedBy` or day grouping.
Supply the count explicitly. Extend the repository with a method that returns
the participant's active-booking count for the target UTC day, for example
`CountActiveBookingsForParticipantOnDayAsync(string bookedBy, DateOnly dayUtc, CancellationToken ct)`,
and compute the count in the query rather than loading entities.

### R10 - capacity-dependent maximum duration

Exact rule from `ASSIGNMENT.md` (verify the current wording before relying on it):

> Rooms with a capacity greater than 10 can be booked for at most 2 hours
> (exactly 2 hours is allowed). A longer booking is rejected with 400. Rooms
> with capacity 10 or less keep the 4-hour limit from R6.

Implementation:
- This refines R6; it is not a separate pipeline stage. Compute the maximum from capacity and apply it at the existing duration step.
- `roomCapacity > 10` -> maximum 2 hours; otherwise 4 hours. Both bounds are inclusive.
- Reject when `range.EndUtc - range.StartUtc > maxDuration`.
- This is a `Validation` error (400), the same classification as R6.
- The domain already receives `roomCapacity`, so no new factory input or repository method is needed.
- Reuse the single `Booking.DurationExceeded` error; the capacity threshold (10) is the only new constant.

### R11 - cancellation lead-time guard

Exact rule from `ASSIGNMENT.md` (verify the current wording before relying on it):

> A booking can be cancelled only if it starts at least 60 minutes from now
> (current time from `TimeProvider`). Otherwise the cancel request is rejected
> with 409.

Implementation:
- `Cancel` needs the current time, so its signature changes to accept `nowUtc`: `Result<Unit> Cancel(DateTime nowUtc)`.
- Reject when `Range.StartUtc - nowUtc < TimeSpan.FromMinutes(60)`. Exactly 60 minutes is allowed.
- This covers bookings whose start is now or in the past; they are inside the 60-minute window and cannot be cancelled.
- This is a `Conflict` error (409).
- `nowUtc` comes from `TimeProvider` in the CancelBooking handler (`time.GetUtcNow().UtcDateTime`), never `DateTime.UtcNow`.
- Order relative to R8: confirm from the assignment whether an already-cancelled booking inside the lead-time window returns the R8 conflict or the R11 conflict. Unspecified default: check the `Cancelled` state first (R8 `AlreadyCancelled`), then the lead-time guard (R11), since re-cancelling is a clearer state error. State this assumption.

Suggested errors:
- `Booking.BookedByRequired`
- `DateRange.EndBeforeStart`
- `Booking.InvalidAttendees`
- `Booking.AttendeesExceedCapacity`
- `Booking.StartInPast`
- `Booking.DurationExceeded`
- `Booking.RoomOccupied`
- `Booking.DailyLimitReached`
- `Booking.AlreadyCancelled`
- `Booking.CancellationTooLate`
- `Booking.NotFound`

Validation failures are `Validation` errors. Overlap, the per-day limit,
repeated cancellation, and the late-cancellation guard are `Conflict` errors.
Missing resources are `NotFound` errors.

### Cancellation

Implement:

```csharp
public Result<Unit> Cancel(DateTime nowUtc)
```

Behavior:
- `Cancelled` -> Conflict (`BookingErrors.AlreadyCancelled`), checked first.
- Start within 60 minutes of `nowUtc` (`Range.StartUtc - nowUtc < TimeSpan.FromMinutes(60)`) -> Conflict (`BookingErrors.CancellationTooLate`), per R11.
- Otherwise `Active` -> `Cancelled`: success (`Unit.Value`).

The handler supplies `nowUtc` from `TimeProvider`. Only this domain method
changes the cancellation state.

---

## 8. Repositories and Unit of Work

Contracts belong in Domain and must not expose EF Core types or `IQueryable`.

Suggested contracts:

```csharp
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken);

    void Add(Room room);
}

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<DateRange>> GetActiveRangesForRoomAsync(
        Guid roomId, CancellationToken cancellationToken);

    Task<int> CountActiveBookingsForParticipantOnDayAsync(
        string bookedBy, DateOnly dayUtc, CancellationToken cancellationToken);

    void Add(Booking booking);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
```

Implement repositories in Infrastructure using the same scoped `DbContext`.

Rules:
- `Add` marks an entity for insertion; it does not save.
- Cancellation loads a tracked `Booking`.
- Active-range queries filter by both `roomId` and `BookingStatus.Active`.
- The R9 count filters by `bookedBy`, `BookingStatus.Active`, and the UTC date of `StartUtc`, and returns a count computed in the database (`CountAsync`) rather than materializing bookings. Compare the stored date using the same UTC normalization the write path applies.
- Read-only interval queries may use `AsNoTracking()` (verify whether projecting `.Select(b => b.Range)` or `.Select(b => new { b.Range.StartUtc, b.Range.EndUtc })` cleanly applies `UtcDateTimeConverter` in the target EF Core version).
- `DbContext` itself implements `IUnitOfWork`.
- Do not create a separate `UnitOfWork` class.

The load/check/save flow alone does not establish concurrency-safe overlap
prevention. Do not claim otherwise. If concurrent booking guarantees are
required, address them explicitly and test them rather than assuming the
domain check provides atomicity.

---

## 9. EF Core and SQLite

Use EF Core packages compatible with the project's target framework.
Keep related package versions consistent; avoid unrelated upgrades.

### DbContext and mappings

`RoomHubDbContext`:
- Inherits `DbContext`.
- Implements `IUnitOfWork`.
- Exposes `Rooms` and `Bookings` `DbSet`s.
- Calls `ApplyConfigurationsFromAssembly(typeof(RoomHubDbContext).Assembly)`.

Use one `IEntityTypeConfiguration<T>` per entity.

Configure:
- Tables `Rooms` and `Bookings`.
- `Guid` primary keys with `ValueGeneratedNever()`.
- Required fields.
- `BookingStatus` stored as text (`.HasConversion<string>()`).
- Room foreign key (`HasOne<Room>().WithMany().HasForeignKey(b => b.RoomId)`).
- A deletion behavior consistent with the assignment; otherwise restrict
  deletion of rooms with bookings.

Use `ComplexProperty` for `DateRange` in the `Bookings` table:

```csharp
builder.ComplexProperty(b => b.Range, range =>
{
    range.Property(r => r.StartUtc)
        .HasColumnName("StartUtc")
        .HasConversion<UtcDateTimeConverter>();

    range.Property(r => r.EndUtc)
        .HasColumnName("EndUtc")
        .HasConversion<UtcDateTimeConverter>();
});
```

Follow the lecture's converter:

```csharp
public sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(
            value, DateTimeKind.Utc));
```

UTC normalization happens before persistence. The read conversion restores
the UTC `Kind`; it is not a substitute for normalizing incoming timestamps.

Do not add public setters to simplify EF mapping.

### DI

Register:
- `DbContext` as Scoped.
- Repositories as Scoped.
- Command handlers as Scoped.
- `TimeProvider.System` as Singleton.
- `JsonStringEnumConverter` in HTTP JSON options.

Resolve `IUnitOfWork` to the existing scoped context:

```csharp
builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<RoomHubDbContext>());
```

Read the SQLite connection string from configuration (check `appsettings.json` and `ASSIGNMENT.md` for the expected key, e.g., `"RoomHub"` or `"DefaultConnection"`). Configure it through the `AddDbContext` service-provider overload demonstrated in Lecture 04 so configuration can be overridden by tests:

```csharp
builder.Services.AddDbContext<RoomHubDbContext>((serviceProvider, options) =>
    options.UseSqlite(
        serviceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("RoomHub")));
```

### Initialization and persistence

Use a file-backed database, not an in-memory SQLite connection.

Initialize within an application startup scope:
- With migrations: `MigrateAsync`.
- Without migrations: `EnsureCreatedAsync`.

```csharp
if (db.Database.GetMigrations().Any())
    await db.Database.MigrateAsync();
else
    await db.Database.EnsureCreatedAsync();
```

The course shows selecting between these based on migration availability.
Do not call both sequentially on the same database.

Do not assume a database created by `EnsureCreated` can later be upgraded
using migrations without a transition plan.

Do not delete persistent data on normal startup.

Seed only if needed:
- Development only.
- Idempotently.
- Through successful domain factories.
- Without changing assumptions made by acceptance tests.

Ignore generated database files and sidecars (`.db`, `.db-shm`, `.db-wal`) in Git. Follow the actual assignment's migration-submission requirements rather than treating the lecture repository's migration policy as universal.

---

## 10. CQRS commands and handlers

Implement the Lecture 06 contracts:

```csharp
public interface ICommand<TResult>
{
}

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(
        TCommand command,
        CancellationToken cancellationToken);
}
```

Required commands:
- `CreateRoomCommand`
- `CreateBookingCommand`
- `CancelBookingCommand`

Creation handlers may return a domain entity or a small use-case result.
Keep HTTP `Response` types owned by their endpoint slices.

### Handler flows

**CreateRoom**
1. Call `Room.Create`.
2. Return failure if present.
3. Add room.
4. Save through `IUnitOfWork`.
5. Return success.

**CreateBooking**
1. Load room.
2. Return `RoomErrors.NotFound` if missing.
3. Load active intervals for the room.
4. Obtain `time.GetUtcNow().UtcDateTime`.
5. Load the participant's active-booking count for the UTC day of the normalized `startUtc` (R9).
6. Call `Booking.Create` with all required data, including the active-range set and the same-day count.
7. Return failure if present.
8. Add booking and save.
9. Return success.

**CancelBooking**
1. Load tracked booking.
2. Return `BookingErrors.NotFound` if missing.
3. Obtain `time.GetUtcNow().UtcDateTime`.
4. Call `booking.Cancel(nowUtc)` (enforces R8 and R11).
5. Return failure if present.
6. Save.
7. Return `Unit.Value`.

Handlers must not:
- Duplicate domain validation.
- Set entity state directly.
- Query `DbContext` directly.
- Construct HTTP responses or `ProblemDetails`.
- Save after a failed domain operation.

Pass `CancellationToken` through asynchronous operations.

### Registration

Implement `AddHandlers(this IServiceCollection services, Assembly assembly)`:
- Scan once during startup.
- Find concrete, non-abstract, closed handler implementations (`type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false }`).
- Select constructed `ICommandHandler<,>` interfaces (`i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)`).
- Register each interface/implementation pair as Scoped.

In `Program.cs`:

```csharp
builder.Services.AddHandlers(typeof(Program).Assembly);
```

Inject `ICommandHandler<TCommand, TResult>` into write endpoints, not the
concrete handler class.

---

## 11. HTTP endpoints and error mapping

Write endpoints perform only:
1. HTTP binding.
2. Command construction.
3. Handler invocation.
4. Result-to-HTTP translation.

Use `TypedResults` with explicit result unions, for example:
- `Results<Created<Response>, ProblemHttpResult>`
- `Results<Ok<Response>, ProblemHttpResult>`
- `Results<NoContent, ProblemHttpResult>`

Centralize errors in:

```csharp
public static ProblemHttpResult ToProblem(this Error error)
{
    var status = error.Type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    return TypedResults.Problem(
        detail: error.Message,
        statusCode: status,
        title: error.Code,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = error.Code
        });
}
```

Use the same mapping for read-side missing resources unless the assignment
requires another error contract.

Read slices:
- May inject `RoomHubDbContext` directly.
- Use `AsNoTracking()`.
- Return the slice's `Response` type.
- May use a simple response projection; do not introduce a query mediator
  or advanced query infrastructure.
- Preserve UTC timestamps and flattened response fields (`StartUtc`, `EndUtc` from `b.Range`).

Keep route composition understandable, especially for the nested
`/api/rooms/{roomId:guid}/bookings` route.

---

## 12. Verification

Implement incrementally. After each coherent change:
1. Build.
2. Resolve introduced errors.
3. Run relevant tests.
4. Exercise affected HTTP requests.
5. Check that the API contract is unchanged.

Use automated tests where practical and maintain `roomhub.http`.
Manual request files are not a substitute for deterministic clock tests.

### Required coverage

| Area | Cases |
|------|-------|
| Rooms | Valid create/get; missing room; blank name; zero/negative capacity; stored name trimmed |
| Booking input | Valid create/get; missing booking; blank `BookedBy`; end before/equal start; zero/negative/excess attendees |
| Precedence | Missing room plus otherwise-invalid booking -> 404 |
| Time | Past start rejected; start exactly now accepted; max duration accepted at the boundary; longer rejected |
| Duration by capacity (R10) | Capacity <= 10 allows 4h, rejects >4h (400); capacity > 10 allows 2h, rejects >2h (400) |
| Overlap | Partial overlap and identical intervals rejected; touching intervals accepted; different rooms independent |
| Daily limit (R9) | Third active booking same participant/UTC day rejected 409; two allowed; cancelled booking does not count toward the limit; different days independent; different participants independent; limit spans all rooms |
| Cancellation | First cancellation 204 with no body; repeated cancellation 409; missing booking 404; cancelled interval reusable; cancel within 60 min of start rejected 409 (R11); cancel at exactly 60 min allowed |
| UTC | Supported `DateTime` kinds normalized; persisted/read timestamps retain UTC semantics |
| Persistence | Created room and booking remain retrievable after restart |
| HTTP | Correct JSON fields, enum strings, status codes, `ProblemDetails`, and `Location` headers |

Use a fixed `TimeProvider` or explicit fixed `nowUtc` for exact-boundary tests.
A live HTTP request cannot reliably test "exactly now" against a moving
system clock.

Run `dotnet test` if tests exist. Do not claim that no discovered tests
means business behavior has been tested.

Fix introduced warnings and investigate existing warnings. Do not turn
"zero warnings" into an invented grading requirement.

Report precisely:
- What was changed.
- What commands/tests ran.
- What passed or failed.
- What remains unverified.

---

## 13. Documentation and submission

Read the actual authorship requirements first.

For this RoomHub workflow, preserve the student-authorship restriction
described in the assignment context:
- Do not author complete submission-ready `BUGS.md` or `DECISIONS.md` answers
  on the student's behalf.
- Offer questions, outlines, factual evidence, and feedback on student drafts.
- Record only actual interactions and verification in `AI-LOG.md`.
- Never fabricate prompts, timestamps, rejected suggestions, or test results.

Useful evidence to surface:

**BUGS.md**
- Actual file and location.
- Reproduction.
- Root cause.
- Applied correction.
- Verification performed.

**DECISIONS.md**
- Domain placement of rules.
- `Result` versus expected business exceptions.
- Write repositories versus direct read-side `DbContext`.
- Shared Unit of Work.
- `TimeProvider`.
- Request-to-response flow.
- Suggestions the student actually rejected and why.

**AI-LOG.md**
- Actual request.
- What assistance was used.
- What was changed or rejected.
- What was independently verified.

Verify all grading, timing, commit-count, repository-visibility, and
submission requirements from `ASSIGNMENT.md`.

If confirmed, plan commits and time accordingly. Do not invent rubric
points or claim that pushing, publication, or submission succeeded without
verification.

Do not commit build output, database files, or secrets. Commit and push
only when authorized by the user or established workflow.

---

## 14. New requirements and completion

When an assignment amendment arrives:
1. Read its exact wording.
2. Identify affected domain rules and HTTP contracts.
3. Change domain behavior where appropriate.
4. Add stable errors if needed.
5. Change handler orchestration only when needed.
6. Keep endpoints thin.
7. Add regression coverage and build again.

Do not assume every new requirement is a domain rule; transport and
persistence requirements belong at their appropriate boundaries.

Before declaring completion, verify:
- All five slices exist.
- R1-R11 and required edge cases work.
- Rich entities and `DateRange` protect their state.
- `Result`/`Error` replace expected business exceptions.
- All three write handlers are registered automatically.
- HTTP error mapping is centralized.
- Repositories and Unit of Work share the same scoped `DbContext`.
- SQLite data survives restart.
- The exact HTTP contract is preserved.
- The build succeeds and actual test results are reported.
- Documentation and submission obligations are accurately identified.

Prefer the simplest understandable implementation that satisfies the
assignment and demonstrates lectures 01-06. Do not trade a working build
for unnecessary architectural polish.