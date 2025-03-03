using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MyAppApi.Models;
using MyAppApi.Data.Dtos;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IEnumerable<CarDto>>> GetCars(string location = "")
        {
            var carsQuery = _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Location)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var cars = await carsQuery.ToListAsync();

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
                Location = car.Location.Name,
                ImageUrl = car.Images.Any() ? $"/images/{car.Images.First().Url}" : "/images/default-image.jpg",
                Status = car.Status
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
                Location = car.Location != null ? $"{car.Location.Name}, {car.Location.Name}" : "Unknown",
                ImageUrl = car.Images.Any() ? $"/images/{car.Images.First().Url}" : "/images/default-image.jpg",
                Status = car.Status
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

        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
        {
            var locations = await _context.Locations
                .Select(l => new LocationDto { Id = l.Id, Name = l.Name })
                .ToListAsync();

            return Ok(locations);
        }

        [HttpGet("all-cars")]
        public async Task<IActionResult> GetAllCarsForAdmin()
        {
            var cars = await _context.Cars
                .Include(c => c.Location)
                .Include(c => c.Images)
                .ToListAsync();

            return Ok(cars);
        }

        [HttpPut("update-car/{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarUpdateDto carDto)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound("Car not found.");


            if (carDto.Status != CarStatus.Available && carDto.Status != CarStatus.Unavailable)
            {
                return BadRequest("Status can only be changed to Available or Unavailable.");
            }

            car.Brand = carDto.Brand;
            car.Model = carDto.Model;
            car.Year = carDto.Year;
            car.Price = carDto.Price;
            car.Status = carDto.Status;

            await _context.SaveChangesAsync();
            return Ok("Car updated successfully.");
        }
        [HttpGet("car-stats/{carId}/{year}/{month}/{today?}")]
        public async Task<IActionResult> GetCarBookingStats(int carId, int year, int month, bool today = false)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = today ? DateTime.Today : monthStart.AddMonths(1).AddDays(-1); 

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Where(b => b.CarId == carId &&
                            b.BookingStatus == "Accepted" &&
                            (b.StartDate <= monthEnd && b.EndDate >= monthStart)) 
                .Select(b => new StateDto
                {
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    UserId = b.UserId,
                    UserName = b.User != null ? b.User.UserName : "Not Found",
                    TotalPrice = b.TotalPrice,
                    Price = b.Car != null ? b.Car.Price : 0 
                })
                .ToListAsync();

            return Ok(bookings);
        }





        public class LocationDto
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class StateDto
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
        }


    }
}