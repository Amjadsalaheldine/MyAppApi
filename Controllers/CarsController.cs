
using MyAppApi.Data.Dtos;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Endpoint: GET api/cars
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
                ImageUrl = car.Images.Any() ? car.Images.First().Url : "default-image-url.jpg"
            }).ToList();

            return Ok(carDtos);
        }

        // ✅ Endpoint: POST api/cars (إضافة سيارة جديدة)
        [HttpPost]
        public async Task<IActionResult> AddCar([FromBody] CarCreateDto carDto)
        {
            if (carDto == null)
                return BadRequest("Invalid data.");

            // 🔹 التحقق من الموقع قبل إضافة السيارة
            var location = await _context.Locations.FindAsync(carDto.LocationId);
            if (location == null)
                return NotFound("Location not found.");

            // 🔹 إنشاء السيارة الجديدة
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
            await _context.SaveChangesAsync(); // 🔹 حفظ السيارة أولاً للحصول على CarId

            // 🔹 إضافة الصور المرتبطة
            if (carDto.ImageUrls != null && carDto.ImageUrls.Count > 0)
            {
                foreach (var imageUrl in carDto.ImageUrls)
                {
                    _context.Images.Add(new Image
                    {
                        Url = imageUrl,
                        CarId = newCar.Id
                    });
                }

                await _context.SaveChangesAsync(); // 🔹 حفظ الصور
            }

            return CreatedAtAction(nameof(GetCars), new { id = newCar.Id }, newCar);
        }
    }
}
