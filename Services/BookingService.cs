using MyAppApi.Data.Dtos;
using MyAppApi.Models;
using Microsoft.EntityFrameworkCore;

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
            var booking = new Booking
            {
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate,
                TotalPrice = bookingDto.TotalPrice,
                UserId = bookingDto.UserId,
                CarId = bookingDto.CarId,
                IdentityImage = bookingDto.IdentityImage,
                BookingStatus = bookingDto.BookingStatus
            };

            _context.Bookings.Add(booking);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<List<BookingDto>> GetBookingsByUserAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalPrice = b.TotalPrice,
                    UserId = b.UserId,
                    CarId = b.CarId,
                    IdentityImage = b.IdentityImage,
                    BookingStatus = b.BookingStatus
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<List<BookingDto>> GetBookingsByCarAsync(int carId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.CarId == carId)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalPrice = b.TotalPrice,
                    UserId = b.UserId,
                    CarId = b.CarId,
                    IdentityImage = b.IdentityImage,
                    BookingStatus = b.BookingStatus
                })
                .ToListAsync();

            return bookings;
        }
    }
}