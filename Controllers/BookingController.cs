using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Data.Dtos;
using MyAppApi.Models;
using MyAppApi.Services;
using System.IO;
using System.Threading.Tasks;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly AppDbContext _context;

        public BookingController(IBookingService bookingService, AppDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromForm] BookingDto bookingDto, IFormFile identityImage)
        {
            if (bookingDto == null || identityImage == null)
            {
                return BadRequest("Booking details and Identity Image are required.");
            }

          
            if (bookingDto.StartDate == default || bookingDto.EndDate == default)
            {
                return BadRequest("Please provide valid start and end dates.");
            }

         
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(identityImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only images are allowed.");
            }

          
            var maxFileSize = 5 * 1024 * 1024; 
            if (identityImage.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the maximum allowed size (5 MB).");
            }

          
            bool isCarBooked = await _context.Bookings.AnyAsync(b =>
                b.CarId == bookingDto.CarId &&
                b.BookingStatus == "Accepted" &&
                b.StartDate < bookingDto.EndDate &&
                bookingDto.StartDate < b.EndDate);

            if (isCarBooked)
            {
                return BadRequest("This car is already booked for the selected dates.");
            }

            try
            {
               
                var uploadsFolder = Path.Combine("wwwroot", "identity_images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await identityImage.CopyToAsync(stream);
                }

                bookingDto.IdentityImage = $"/identity_images/{fileName}";

              
                var result = await _bookingService.CreateBookingAsync(bookingDto);

                if (result)
                {
                
                    var car = await _context.Cars.FindAsync(bookingDto.CarId);
                    if (car != null)
                    {
                        car.Status = CarStatus.Booked;
                        await _context.SaveChangesAsync();
                    }

                    return Ok("Booking successfully created.");
                }
                else
                {
                    return BadRequest("Failed to create booking.");
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"An error occurred while creating the booking: {ex.Message}");
            }
        }
    [HttpGet("user/{userId}")]
public async Task<ActionResult<List<BookingDto>>> GetBookingsByUserId(string userId)
{
    var bookings = await _context.Bookings
        .Where(b => b.UserId == userId)
        .Include(b => b.Car)
            .ThenInclude(c => c.Model)
                .ThenInclude(m => m.Brand)
        .Include(b => b.Payments)
        .Include(b => b.User)
        .ToListAsync();

    if (bookings == null || !bookings.Any())
    {
        return NotFound("No bookings found.");
    }

    var bookingDtos = bookings.Select(b => new BookingDto
    {
        Id = b.Id,
        UserName = b.User?.UserName ?? "Unknown",
        StartDate = b.StartDate,
        EndDate = b.EndDate,
        TotalPrice = b.TotalPrice,
        UserId = b.UserId,
        CarId = b.CarId,
        IdentityImage = b.IdentityImage,
        BookingStatus = b.BookingStatus,

        ModelId = b.Car?.Model?.Id,
        ModelName = b.Car?.Model?.Name,
        BrandId = b.Car?.Model?.Brand?.Id,
        BrandName = b.Car?.Model?.Brand?.Name,

        PaidAmount = b.Payments?.Sum(p => p.Amount) ?? 0,
        RemainingAmount = b.TotalPrice - (b.Payments?.Sum(p => p.Amount) ?? 0)
    }).ToList();

    return Ok(bookingDtos);
}



        [HttpGet("{carId}")]
        public async Task<IActionResult> GetCarById(int carId)
        {
            var car = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .FirstOrDefaultAsync(c => c.Id == carId);

            if (car == null)
            {
                return NotFound("Car not found");
            }

            return Ok(car);
        }

      
        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return NotFound("Booking not found");

            if (booking.BookingStatus != "Pending")
                return BadRequest("Only 'Pending' bookings can be canceled.");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            await CheckAndUpdateCarStatus(booking.CarId);

            return Ok("Booking canceled successfully.");
        }


        private async Task CheckAndUpdateCarStatus(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return;

            bool hasActiveBookings = await _context.Bookings.AnyAsync(b =>
                b.CarId == carId &&
                b.BookingStatus == "Accepted" &&
                b.EndDate >= DateTime.UtcNow);

            car.Status = hasActiveBookings ? CarStatus.Booked : CarStatus.Available;
            await _context.SaveChangesAsync();
        }
    }
}
