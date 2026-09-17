namespace RoomBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AttendeeCount { get; set; }
        public Guid? SeriesId { get; set; }
    }
}