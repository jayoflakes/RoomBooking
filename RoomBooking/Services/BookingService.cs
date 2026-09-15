using Microsoft.EntityFrameworkCore;
using RoomBooking.Data;
using RoomBooking.Models;

namespace RoomBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly RoomBookingContext _context;

        public BookingService(RoomBookingContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateBookingAsync(Booking booking)
        {
            // Rule 1: end time must be after start time
            if (booking.EndTime <= booking.StartTime)
            {
                return (false, "End time must be after start time.");
            }

            // Rule 2: must fall within business hours, Monday to Friday, 08:00-18:00
            if (booking.StartTime.DayOfWeek == DayOfWeek.Saturday ||
                booking.StartTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return (false, "Bookings must be on a weekday.");
            }

            var businessStart = booking.StartTime.Date.AddHours(8);
            var businessEnd = booking.StartTime.Date.AddHours(18);

            if (booking.StartTime < businessStart || booking.EndTime > businessEnd)
            {
                return (false, "Bookings must fall within business hours (08:00-18:00).");
            }

            // Rule 3: attendee count must not exceed room capacity
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            if (booking.AttendeeCount > room.Capacity)
            {
                return (false, $"Attendee count exceeds room capacity ({room.Capacity}).");
            }

            // Rule 4: no overlapping bookings for the same room
            bool overlaps = await _context.Bookings
                .Where(b => b.RoomId == booking.RoomId)
                .AnyAsync(b => booking.StartTime < b.EndTime && booking.EndTime > b.StartTime);

            if (overlaps)
            {
                return (false, "This room is already booked for part of that time.");
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<List<Room>> FindAvailableRoomsAsync(DateTime start, DateTime end, int attendeeCount)
        {
            var bigEnoughRooms = await _context.Rooms
                .Where(r => r.Capacity >= attendeeCount)
                .ToListAsync();

            var availableRooms = new List<Room>();

            foreach (var room in bigEnoughRooms)
            {
                bool hasConflict = await _context.Bookings
                    .Where(b => b.RoomId == room.Id)
                    .AnyAsync(b => start < b.EndTime && end > b.StartTime);

                if (!hasConflict)
                {
                    availableRooms.Add(room);
                }
            }

            return availableRooms;
        }

    }
}