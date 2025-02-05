using MyAppApi.Models;
using MyAppApi.Data.Dtos;

namespace MyAppApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateBookingAsync(BookingDto bookingDto)
        {
            var newBooking = new Booking
            {
                StartDate = bookingDto.StartDate,
                StartTime = bookingDto.StartTime,
                EndDate = bookingDto.EndDate,
                EndTime = bookingDto.EndTime,
                TotalPrice = bookingDto.TotalPrice,
                UserId = bookingDto.UserId, 
                CarId = bookingDto.CarId,
                IdentityImage = bookingDto.IdentityImage,
                BookingStatus = bookingDto.BookingStatus
            };

            _context.Bookings.Add(newBooking);

            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<List<BookingDto>> GetBookingsByUserAsync(string userId) 
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)  
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    StartDate = b.StartDate,
                    StartTime = b.StartTime,
                    EndDate = b.EndDate,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    CarModel = b.Car.Model,
                    BookingStatus = b.BookingStatus
                })
                .ToListAsync();
        }

       
    }
}
