using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Data.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


[Route("api/[controller]")]
[ApiController]
public class ManageBookingController : ControllerBase
{
    private readonly AppDbContext _context;

    public ManageBookingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManageBookingDto>>> GetBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Car)
            .Include(b => b.User)
            .Select(b => new ManageBookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.UserName,
                IdentityImage = b.IdentityImage,
                CarId = b.CarId,
                CarModel = b.Car.Model,
                TotalPrice = b.TotalPrice,
                BookingStatus = b.BookingStatus,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        booking.BookingStatus = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Booking status updated successfully." });
    }
}

public class ManageBookingDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string IdentityImage { get; set; }
    public int CarId { get; set; }
    public string CarModel { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookingStatus { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class UpdateBookingStatusDto
{
    public string Status { get; set; }
}
