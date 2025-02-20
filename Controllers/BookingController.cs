using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;
using MyAppApi.Data.Dtos;
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
                return BadRequest("Booking details and Identity Image are required");
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

            
            var uploadsFolder = Path.Combine("wwwroot", "identity_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(identityImage.FileName)}";
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

                return Ok("Booking successfully created");
            }

            return BadRequest("Failed to create booking");
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BookingDto>>> GetBookingsByUserId(string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Car)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound("No bookings found.");
            }

            var bookingDtos = bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                UserId = b.UserId,
                CarId = b.CarId,
                IdentityImage = b.IdentityImage,
                BookingStatus = b.BookingStatus
            }).ToList();

            return Ok(bookingDtos);
        }

        [HttpGet("car/{carId}")]
        public async Task<IActionResult> GetBookingsByCar(int carId)
        {
            await CheckAndUpdateCarStatus(carId); 

            var bookings = await _bookingService.GetBookingsByCarAsync(carId);

            if (bookings == null || !bookings.Any())
            {
                return NotFound("No bookings found for this car");
            }

            return Ok(bookings);
        }

        [HttpPost("cancel-booking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return NotFound("Booking not found");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            
            await CheckAndUpdateCarStatus(booking.CarId);

            return Ok("Booking canceled, car status updated");
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
