
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MyAppApi.Models;
using MyAppApi.Data.Dtos;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CarsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetCars()
        {
            var cars = await _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Location)
                .ToListAsync();

            if (cars == null || !cars.Any())
            {
                return NotFound("No cars found.");
            }

            var carDtos = cars.Select(car => new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Location = car.Location != null ? $"{car.Location.City}, {car.Location.Country}" : "Unknown",
                ImageUrl = car.Images.Any() ? $"/images/{car.Images.First().Url}" : "/images/default-image.jpg"
            }).ToList();

            return Ok(carDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCar(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            var carDto = new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Location = car.Location != null ? $"{car.Location.City}, {car.Location.Country}" : "Unknown",
                ImageUrl = car.Images.Any() ? $"/images/{car.Images.First().Url}" : "/images/default-image.jpg"
            };

            return Ok(carDto);
        }

        [HttpGet("admin/stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            var totalCars = await _context.Cars.CountAsync();
            var bookedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Booked);
            var availableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Available);
            var unavailableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Unavailable);
            var pendingBookings = await _context.Bookings.CountAsync(b => b.BookingStatus == "Pending");
            var totalRevenue = await _context.Bookings.SumAsync(b => b.TotalPrice);

            var stats = new
            {
                TotalCars = totalCars,
                BookedCars = bookedCars,
                AvailableCars = availableCars,
                UnavailableCars = unavailableCars,
                PendingBookings = pendingBookings,
                TotalRevenue = totalRevenue
            };

            return Ok(stats);
        }




        [HttpPost]
        public async Task<IActionResult> AddCar([FromForm] CarCreateDto carDto)
        {
            if (carDto == null || carDto.Images == null || !carDto.Images.Any())
                return BadRequest("Invalid data or no images provided.");

            var location = await _context.Locations.FindAsync(carDto.LocationId);
            if (location == null)
                return NotFound("Location not found.");

            var newCar = new Car
            {
                Model = carDto.Model,
                Brand = carDto.Brand,
                Year = carDto.Year,
                Price = carDto.Price,
                LocationId = carDto.LocationId,
                Images = new List<Image>()
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRootPath, "images");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var image in carDto.Images)
            {
                var fileName = $"{newCar.Id}_{Path.GetRandomFileName()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                _context.Images.Add(new Image { Url = fileName, CarId = newCar.Id });
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCars), new { id = newCar.Id }, newCar);
        }


    }
}
