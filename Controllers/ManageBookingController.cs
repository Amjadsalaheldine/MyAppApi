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
                .ThenInclude(c => c.Model) 
            .Include(b => b.Car)
                .ThenInclude(c => c.Brand) 
            .Include(b => b.User)
            .Select(b => new ManageBookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.UserName,
                IdentityImageUrl = !string.IsNullOrEmpty(b.IdentityImage)
    ? $"/identity_images/{Path.GetFileName(b.IdentityImage)}" 
    : null,

                CarId = b.CarId,
                CarModel = b.Car.Model.Name, 
                CarBrand = b.Car.Brand.Name, 
                TotalPrice = b.TotalPrice,
                BookingStatus = b.BookingStatus,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
            })
            .ToListAsync();

        return Ok(bookings);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<ManageBookingDto>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Car)
                .ThenInclude(c => c.Model)
            .Include(b => b.Car)
                .ThenInclude(c => c.Brand)
            .Include(b => b.User)
            .Where(b => b.Id == id)
            .Select(b => new ManageBookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                IdentityImageUrl = !string.IsNullOrEmpty(b.IdentityImage)
    ? $"/identity_images/{Path.GetFileName(b.IdentityImage)}" 
    : null,
                CarId = b.CarId,
                CarModel = b.Car.Model.Name,
                CarBrand = b.Car.Brand.Name,
                TotalPrice = b.TotalPrice,
                BookingStatus = b.BookingStatus,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
            })
            .FirstOrDefaultAsync();

        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        return Ok(booking);
    }

 
    [HttpGet("identity-image/{imagePath}")]
    public IActionResult GetIdentityImage(string imagePath)
    {
        var filePath = Path.Combine("wwwroot", "identity_images", imagePath);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Image not found.");
        }

        var imageFileStream = System.IO.File.OpenRead(filePath);
        return File(imageFileStream, "image/jpeg"); 
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

  
    public string IdentityImageUrl { get; set; }

    public int CarId { get; set; }
    public string CarModel { get; set; } 
    public string CarBrand { get; set; } 
    public decimal TotalPrice { get; set; }
    public string BookingStatus { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}



public class UpdateBookingStatusDto
{
    public string Status { get; set; }
}