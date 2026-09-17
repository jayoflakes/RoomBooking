using Microsoft.EntityFrameworkCore;
using RoomBooking.Data;
using RoomBooking.Models;
using RoomBooking.Services;
using Xunit;

namespace RoomBooking.Tests
{
    public class BookingServiceTests
    {
        private RoomBookingContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RoomBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RoomBookingContext(options);
        }

        [Fact]
        public async Task CreateBooking_Succeeds_WhenValid()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var booking = new Booking
            {
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = new DateTime(2026, 9, 14, 9, 0, 0),  // a Monday
                EndTime = new DateTime(2026, 9, 14, 10, 0, 0),
                AttendeeCount = 5
            };

            // Act
            var (success, errorMessage) = await service.CreateBookingAsync(booking);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
        }


        [Fact]
        public async Task CreateBooking_Fails_WhenEndBeforeStart()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var booking = new Booking
            {
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = new DateTime(2026, 9, 14, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 14, 9, 0, 0), // ends before it starts
                AttendeeCount = 5
            };

            var (success, errorMessage) = await service.CreateBookingAsync(booking);

            Assert.False(success);
            Assert.Equal("End time must be after start time.", errorMessage);
        }

        [Fact]
        public async Task CreateBooking_Fails_OutsideBusinessHours()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var booking = new Booking
            {
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = new DateTime(2026, 9, 14, 19, 0, 0), // 7pm, after hours
                EndTime = new DateTime(2026, 9, 14, 20, 0, 0),
                AttendeeCount = 5
            };

            var (success, errorMessage) = await service.CreateBookingAsync(booking);

            Assert.False(success);
            Assert.Equal("Bookings must fall within business hours (08:00-18:00).", errorMessage);
        }

        [Fact]
        public async Task CreateBooking_Fails_WhenAttendeesExceedCapacity()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Small Room", Capacity = 2 });
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var booking = new Booking
            {
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = new DateTime(2026, 9, 14, 9, 0, 0),
                EndTime = new DateTime(2026, 9, 14, 10, 0, 0),
                AttendeeCount = 5 // exceeds capacity of 2
            };

            var (success, errorMessage) = await service.CreateBookingAsync(booking);

            Assert.False(success);
            Assert.Equal("Attendee count exceeds room capacity (2).", errorMessage);
        }

        [Fact]
        public async Task CreateBooking_Fails_WhenOverlapping()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });
            context.Bookings.Add(new Booking
            {
                RoomId = 1,
                OrganizerName = "Bob",
                StartTime = new DateTime(2026, 9, 14, 9, 0, 0),
                EndTime = new DateTime(2026, 9, 14, 10, 0, 0),
                AttendeeCount = 3
            });
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var overlappingBooking = new Booking
            {
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = new DateTime(2026, 9, 14, 9, 30, 0), // overlaps Bob's 9-10 booking
                EndTime = new DateTime(2026, 9, 14, 10, 30, 0),
                AttendeeCount = 2
            };

            var (success, errorMessage) = await service.CreateBookingAsync(overlappingBooking);

            Assert.False(success);
            Assert.Equal("This room is already booked for part of that time.", errorMessage);
        }

        [Fact]
        public async Task FindAvailableRooms_ExcludesRoomsTooSmallOrBooked()
        {
            var context = CreateInMemoryContext();

            context.Rooms.Add(new Room { Id = 1, Name = "Small Room", Capacity = 2 });      // too small
            context.Rooms.Add(new Room { Id = 2, Name = "Big Room", Capacity = 10 });       // big enough, but booked
            context.Rooms.Add(new Room { Id = 3, Name = "Free Room", Capacity = 10 });      // big enough and free

            context.Bookings.Add(new Booking
            {
                RoomId = 2,
                OrganizerName = "Bob",
                StartTime = new DateTime(2026, 9, 14, 9, 0, 0),
                EndTime = new DateTime(2026, 9, 14, 10, 0, 0),
                AttendeeCount = 4
            });

            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var availableRooms = await service.FindAvailableRoomsAsync(
                start: new DateTime(2026, 9, 14, 9, 30, 0),  // overlaps Big Room's existing booking
                end: new DateTime(2026, 9, 14, 10, 30, 0),
                attendeeCount: 5);

            Assert.Single(availableRooms);
            Assert.Equal("Free Room", availableRooms[0].Name);
        }

        [Fact]
        public async Task CancelBooking_Fails_WhenAlreadyStarted()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });

            var pastBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = DateTime.Now.AddHours(-2), // started 2 hours ago
                EndTime = DateTime.Now.AddHours(-1),
                AttendeeCount = 3
            };
            context.Bookings.Add(pastBooking);
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var (success, errorMessage) = await service.CancelBookingAsync(1);

            Assert.False(success);
            Assert.Equal("This booking has already started and can no longer be cancelled.", errorMessage);
        }

        [Fact]
        public async Task CancelBooking_Succeeds_WhenNotYetStarted()
        {
            var context = CreateInMemoryContext();
            context.Rooms.Add(new Room { Id = 1, Name = "Boardroom", Capacity = 10 });

            var futureBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                OrganizerName = "Alice",
                StartTime = DateTime.Now.AddHours(2), // starts 2 hours from now
                EndTime = DateTime.Now.AddHours(3),
                AttendeeCount = 3
            };
            context.Bookings.Add(futureBooking);
            await context.SaveChangesAsync();

            var service = new BookingService(context);

            var (success, errorMessage) = await service.CancelBookingAsync(1);

            Assert.True(success);
            Assert.Null(errorMessage);
        }

    }

}