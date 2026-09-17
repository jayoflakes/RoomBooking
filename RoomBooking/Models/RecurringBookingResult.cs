namespace RoomBooking.Models
{
    public class RecurringBookingResult
    {
        public int WeekNumber { get; set; }
        public DateTime StartTime { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}