# Meeting Room Booking Service

A small ASP.NET Core MVC application for booking meeting rooms in an office. Built as a learning project to understand ASP.NET Core MVC, Entity Framework Core, SQL Server, and general full-stack web development.

## Tech stack

- **ASP.NET Core MVC** (.NET 8)
- **Entity Framework Core** (SQL Server provider) as the ORM
- **SQL Server** (Developer Edition, local instance)
- **xUnit** for automated tests, with the EF Core InMemory provider for test isolation

## Prerequisites

- Visual Studio 2022 (or later) with the ASP.NET and web development workload
- SQL Server Developer Edition or Express, installed locally (no Docker used in this project)
- SQL Server Management Studio (SSMS), for running the schema script

## Setup

### 1. Install SQL Server locally

Install SQL Server Developer Edition (free for dev/test use). During installation, a default or named instance will be created — take note of the instance name if it's not the default (`MSSQLSERVER`).

### 2. Create the database and schema

Open SSMS and connect to your local instance (Windows Authentication is used throughout this project, so no SQL login is required).

Create a new database called `RoomBookingDb`, then run the following script against it (also included as `schema.sql` in this repo):

```sql
CREATE TABLE Rooms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Capacity INT NOT NULL CHECK (Capacity > 0)
);

CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL,
    OrganizerName NVARCHAR(200) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    AttendeeCount INT NOT NULL CHECK (AttendeeCount > 0),
    CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    CONSTRAINT CHK_EndAfterStart CHECK (EndTime > StartTime)
);
```

Optionally, seed a couple of rooms to have something to test with:

```sql
INSERT INTO Rooms (Name, Capacity) VALUES
('Boardroom', 12),
('Focus Pod A', 2),
('The Hive', 6);
```

### 3. Configure the connection string

The connection string lives in `appsettings.json`:

```json
"ConnectionStrings": {
  "RoomBookingDb": "Server=localhost;Database=RoomBookingDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

If your SQL Server instance is named (e.g. `localhost\SQLEXPRESS`), update `Server=localhost` accordingly.

### 4. Run the application

Open `RoomBooking.sln` in Visual Studio and press **F5** (or `dotnet run` from the `RoomBooking` project folder). The app will launch in your browser at `https://localhost:{port}`.

Main pages:
- `/Rooms` — list and create rooms
- `/Bookings` — list, create, and cancel bookings
- `/Bookings/ForRoom?roomId={id}&date={yyyy-MM-dd}` — bookings for a specific room and date (defaults to today if no date is given)
- `/Bookings/Availability` — search for available rooms given a date, time range, and attendee count

### 5. Run the tests

Open **Test Explorer** in Visual Studio (**Test → Test Explorer**) and click **Run All**, or from the command line:

```
dotnet test
```

There are 6 automated tests in `RoomBooking.Tests/BookingServiceTests.cs`, covering the booking business rules: successful creation, and rejection for each of end-before-start, outside business hours, over-capacity, and overlapping bookings, plus a test for the availability search logic.

## Decisions made where the spec was unclear

- **Cancelling a booking** — the spec doesn't specify soft-delete vs. hard-delete. This implementation performs a **hard delete** (the row is removed from the database) for simplicity. A soft-delete (status flag) would be a reasonable alternative if an audit trail of cancellations were needed.
- **Listing bookings for a room with no date specified** — defaults to **today's date**.
- **Room deletion** — not implemented, as it wasn't a stated requirement. This means there's currently no handling for what should happen to existing bookings if a room were ever deleted — a known gap rather than an oversight.
- **Availability search** — implemented as a browsable HTML form, but built on a plain `GET` with query-string parameters (`date`, `startTime`, `endTime`, `attendeeCount`), so it also functions as a simple queryable endpoint without requiring the form UI.

## Write-up

*Claude and Frances guided me throughout the project, so they deserve an honourable mention. Without their help, I would have no idea where to start, and to a large degree, still don't.*

**What was hardest, and why**

The hardest part was remembering what each term means, the sheer volume of information I went through made it difficult to know what I retained and what I didn't, remembering what works with what and how. With Claude and Frances explaining everything, I understood everything but carrying those concepts into the next steps was difficult. The more I learned, the more I knew I had to learn. It was fun, challenging and my eyes are sore.

**What I'd do differently with more time**

If I had more time, I would spend it learning and practising what I learned.

**What I'd cut if I only had 3 days**

If I only had 3 days, I'd cut sleep, showering, and lunch, and survive on caffeine and sheer panic.
