namespace MyAppApi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }

        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        
        public int CarId { get; set; }
        public Car Car { get; set; }

    
        public Payment Payment { get; set; }
    }
}