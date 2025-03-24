using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MyAppApi.Models;
using MyAppApi.Data.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var cars = await carsQuery.ToListAsync();

            if (!cars.Any())
            {
                return NotFound("No cars found.");
            }

            var carDtos = cars.Select(car => new CarDto
            {
                Id = car.Id,
                ModelId = car.Model?.Id,
                ModelName = car.Model?.Name,
                BrandId = car.Model?.Brand?.Id,
                BrandName = car.Model?.Brand?.Name,
                Year = car.Year,
                Price = car.Price,
                LocationId = car.Location.Id,
                LocationName = car.Location.Name,
                Status = car.Status,
                PlateNumber = car.PlateNumber,
                EngineNumber = car.EngineNumber,
                ChassisNumber = car.ChassisNumber,
                Classification = car.Classification,
                CountryOfOrigin = car.CountryOfOrigin,
                Color = car.Color,
                ImageUrls = car.Images.Select(img => $"/images/{img.Url}").ToList()
            }).ToList();

            return Ok(carDtos);
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

        [HttpGet("admin/stats/filtered")]
        public async Task<IActionResult> GetFilteredStats(string location = "")
        {
            var carsQuery = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var totalCars = await carsQuery.CountAsync();
            var bookedCars = await carsQuery.CountAsync(c => c.Status == CarStatus.Booked);
            var availableCars = await carsQuery.CountAsync(c => c.Status == CarStatus.Available);
            var unavailableCars = await carsQuery.CountAsync(c => c.Status == CarStatus.Unavailable);
            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.BookingStatus == "Pending" && (string.IsNullOrEmpty(location) || b.Car.Location.Name.Contains(location)));

            var stats = new
            {
                TotalCars = totalCars,
                BookedCars = bookedCars,
                AvailableCars = availableCars + bookedCars,
                UnavailableCars = unavailableCars,
                PendingBookings = pendingBookings
            };

            return Ok(stats);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCar(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .Include(c => c.Images)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            var carDto = new CarDto
            {
                Id = car.Id,
                ModelId = car.ModelId,
                ModelName = car.Model?.Name ?? "Unknown",
                BrandId = car.Model?.BrandId,
                BrandName = car.Model?.Brand?.Name ?? "Unknown",
                Year = car.Year,
                Price = car.Price,
                LocationId = car.LocationId,
                LocationName = car.Location?.Name ?? "Unknown",
                Status = car.Status,
                PlateNumber = car.PlateNumber,
                EngineNumber = car.EngineNumber,
                ChassisNumber = car.ChassisNumber,
                Classification = car.Classification,
                CountryOfOrigin = car.CountryOfOrigin,
                Color = car.Color,
                ImageUrls = car.Images.Any()
                    ? car.Images.Select(img => $"/images/{img.Url}").ToList()
                    : new List<string> { "/images/default-image.jpg" }
            };

            return Ok(carDto);
        }

      
        [HttpPost]
        public async Task<IActionResult> AddCar([FromForm] CarCreateDto carDto)
        {
            if (carDto == null || carDto.Images == null || !carDto.Images.Any())
                return BadRequest("Invalid data or no images provided.");

            var newCar = new Car
            {
                ModelId = carDto.ModelId,
                BrandId = carDto.BrandId,
                Year = carDto.Year,
                Price = carDto.Price,
                LocationId = carDto.LocationId,
                Status = Enum.TryParse<CarStatus>(carDto.Status, out var status) ? status : CarStatus.Available,
                PlateNumber = carDto.PlateNumber,
                EngineNumber = carDto.EngineNumber,
                ChassisNumber = carDto.ChassisNumber,
                Classification = carDto.Classification,
                CountryOfOrigin = carDto.CountryOfOrigin,
                Color = carDto.Color,
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

        
        [HttpPost("locations")]
        public async Task<IActionResult> AddLocation([FromBody] LocationDto locationDto)
        {
            var location = new Location { Name = locationDto.Name };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, locationDto);
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

            car.ModelId = carDto.ModelId;
            car.BrandId = carDto.BrandId;
            car.Year = carDto.Year;
            car.Price = carDto.Price;
            car.LocationId = carDto.LocationId;
            car.Status = carDto.Status;
            car.PlateNumber = carDto.PlateNumber;
            car.EngineNumber = carDto.EngineNumber;
            car.ChassisNumber = carDto.ChassisNumber;
            car.Classification = carDto.Classification;
            car.CountryOfOrigin = carDto.CountryOfOrigin;
            car.Color = carDto.Color;

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

       
        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new BrandDto { Id = b.Id, Name = b.Name })
                .ToListAsync();

            return Ok(brands);
        }


        [HttpPost("brands")]
        public async Task<ActionResult<BrandDto>> AddBrand([FromBody] BrandDto brandDto)
        {
            if (string.IsNullOrWhiteSpace(brandDto.Name))
            {
                return BadRequest("Brand name is required.");
            }

            var brand = new Brand { Name = brandDto.Name };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return Ok(new BrandDto { Id = brand.Id, Name = brand.Name });
        }


        [HttpGet("models")]
        public async Task<ActionResult<IEnumerable<ModelDto>>> GetModels()
        {
            var models = await _context.Models
                .Include(m => m.Brand)
                .Select(m => new ModelDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    BrandId = m.BrandId,
                    BrandName = m.Brand != null ? m.Brand.Name : ""
                })
                .ToListAsync();

            return Ok(models);
        }

      
        [HttpGet("models/{brandId}")]
        public async Task<IActionResult> GetModelsByBrand(int brandId)
        {
            var models = await _context.Models
                .Where(m => m.BrandId == brandId)
                .Select(m => new ModelDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    BrandId = m.BrandId,
                    BrandName = m.Brand != null ? m.Brand.Name : ""
                })
                .ToListAsync();

            return Ok(models);
        }

       
        [HttpPost("models")]
        public async Task<ActionResult<ModelDto>> AddModel([FromBody] ModelDto modelDto)
        {
            if (string.IsNullOrWhiteSpace(modelDto.Name))
            {
                return BadRequest("Model name is required.");
            }

            var brand = await _context.Brands.FindAsync(modelDto.BrandId);
            if (brand == null)
            {
                return NotFound("Brand not found.");
            }

            var model = new Model
            {
                Name = modelDto.Name,
                BrandId = modelDto.BrandId
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new ModelDto
            {
                Id = model.Id,
                Name = model.Name,
                BrandId = model.BrandId,
                BrandName = brand.Name
            });
        }

      
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCars(string location = "")
        {
            var carsQuery = _context.Cars
                .Where(c => c.Status == CarStatus.Available || c.Status == CarStatus.Booked)
                .Include(c => c.Location)
                .Include(c => c.Images)
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var cars = await carsQuery
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    ModelId = c.ModelId,
                    ModelName = c.Model.Name,
                    BrandId = c.Model.Brand.Id,
                    BrandName = c.Model.Brand.Name,
                    Year = c.Year,
                    Price = c.Price,
                    LocationId = c.LocationId,
                    LocationName = c.Location.Name,
                    Status = c.Status,
                    PlateNumber = c.PlateNumber,
                    EngineNumber = c.EngineNumber,
                    ChassisNumber = c.ChassisNumber,
                    Classification = c.Classification,
                    CountryOfOrigin = c.CountryOfOrigin,
                    Color = c.Color,
                    ImageUrls = c.Images.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Ok(cars);
        }

 
        [HttpGet("booked")]
        public async Task<IActionResult> GetBookedCars(string location = "")
        {
            var carsQuery = _context.Cars
                .Where(c => c.Status == CarStatus.Booked)
                .Include(c => c.Location)
                .Include(c => c.Images)
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var cars = await carsQuery
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    ModelId = c.ModelId,
                    ModelName = c.Model.Name,
                    BrandId = c.BrandId,
                    BrandName = c.Brand.Name,
                    Year = c.Year,
                    Price = c.Price,
                    LocationId = c.LocationId,
                    LocationName = c.Location.Name,
                    Status = c.Status,
                    PlateNumber = c.PlateNumber,
                    EngineNumber = c.EngineNumber,
                    ChassisNumber = c.ChassisNumber,
                    Classification = c.Classification,
                    CountryOfOrigin = c.CountryOfOrigin,
                    Color = c.Color,
                    ImageUrls = c.Images.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Ok(cars);
        }

    
        [HttpGet("unavailable")]
        public async Task<IActionResult> GetUnavailableCars(string location = "")
        {
            var carsQuery = _context.Cars
                .Where(c => c.Status == CarStatus.Unavailable)
                .Include(c => c.Location)
                .Include(c => c.Images)
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                carsQuery = carsQuery.Where(c => c.Location.Name.Contains(location));
            }

            var cars = await carsQuery
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    ModelId = c.ModelId,
                    ModelName = c.Model.Name,
                    BrandId = c.BrandId,
                    BrandName = c.Brand.Name,
                    Year = c.Year,
                    Price = c.Price,
                    LocationId = c.LocationId,
                    LocationName = c.Location.Name,
                    Status = c.Status,
                    PlateNumber = c.PlateNumber,
                    EngineNumber = c.EngineNumber,
                    ChassisNumber = c.ChassisNumber,
                    Classification = c.Classification,
                    CountryOfOrigin = c.CountryOfOrigin,
                    Color = c.Color,
                    ImageUrls = c.Images.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Ok(cars);
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