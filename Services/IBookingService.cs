using MyAppApi.Data.Dtos;

namespace MyAppApi.Services
{
    public interface IBookingService
    {
        Task<bool> CreateBookingAsync(BookingDto bookingDto);
        Task<List<BookingDto>> GetBookingsByUserAsync(string userId);  
        
    }
}
