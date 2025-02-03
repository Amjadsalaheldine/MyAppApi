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


       
    }
}
