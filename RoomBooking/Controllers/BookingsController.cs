using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Data;
using RoomBooking.Models;
using RoomBooking.Services;

namespace RoomBooking.Controllers
{
    public class BookingsController : Controller
    {
        private readonly RoomBookingContext _context;
        private readonly IBookingService _bookingService;

        public BookingsController(RoomBookingContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Rooms = await _context.Rooms.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = await _context.Rooms.ToListAsync();
                return View(booking);
            }

            var (success, errorMessage) = await _bookingService.CreateBookingAsync(booking);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                ViewBag.Rooms = await _context.Rooms.ToListAsync();
                return View(booking);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ForRoom(int roomId, DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Where(b => b.RoomId == roomId && b.StartTime.Date == targetDate.Date)
                .OrderBy(b => b.StartTime)
                .ToListAsync();

            ViewBag.Room = room;
            ViewBag.Date = targetDate;

            return View(bookings);
        }

        public async Task<IActionResult> Availability(DateTime? date, string? startTime, string? endTime, int? attendeeCount)
        {
            if (date == null || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime) || attendeeCount == null)
            {
                return View();
            }

            var start = date.Value.Date + TimeSpan.Parse(startTime);
            var end = date.Value.Date + TimeSpan.Parse(endTime);

            var availableRooms = await _bookingService.FindAvailableRoomsAsync(start, end, attendeeCount.Value);

            ViewBag.SearchPerformed = true;
            ViewBag.Start = start;
            ViewBag.End = end;

            return View(availableRooms);
        }
    }
}