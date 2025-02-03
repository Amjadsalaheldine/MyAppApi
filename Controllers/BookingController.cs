using MyAppApi.Services;
using MyAppApi.Data.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MyAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

       
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto bookingDto)
        {
            if (bookingDto == null)
            {
                return BadRequest("Booking details are required");
            }

            var result = await _bookingService.CreateBookingAsync(bookingDto);

            if (result)
            {
                return Ok("Booking successfully created");
            }

            return BadRequest("Failed to create booking");
        }
    }
}
