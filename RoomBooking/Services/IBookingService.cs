using RoomBooking.Models;

namespace RoomBooking.Services
{
    public interface IBookingService
    {
        Task<(bool Success, string? ErrorMessage)> CreateBookingAsync(Booking booking);
        Task<List<Room>> FindAvailableRoomsAsync(DateTime start, DateTime end, int attendeeCount);
        Task<(bool Success, string? ErrorMessage)> CancelBookingAsync(int bookingId);
        Task<List<RecurringBookingResult>> CreateRecurringBookingAsync(Booking template, int numberOfWeeks);
    }
}