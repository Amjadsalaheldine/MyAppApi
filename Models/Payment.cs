namespace MyAppApi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal RemainingBalance { get; set; } 

       
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
