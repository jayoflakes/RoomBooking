using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Data;
using RoomBooking.Models;

namespace RoomBooking.Controllers
{
    public class RoomsController : Controller
    {
        private readonly RoomBookingContext _context;

        public RoomsController(RoomBookingContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Room room)
        {
            bool nameExists = await _context.Rooms
                .AnyAsync(r => r.Name == room.Name);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(room.Name), "A room with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(room);
            }

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}