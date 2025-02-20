namespace MyAppApi.Models
{
    public class Booking
    {
        public int Id { get; set; }

        
        public DateTime StartDate { get; set; }
     

        
        public DateTime EndDate { get; set; }
       

      
        public decimal TotalPrice { get; set; }

        
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

      
        public int CarId { get; set; }
        public Car ?Car { get; set; }

        
        public string? IdentityImage { get; set; }

        public List<Payment> Payments { get; set; } = new List<Payment>();


        public string? BookingStatus { get; set; }
        
    }
}
