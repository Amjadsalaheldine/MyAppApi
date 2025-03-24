namespace MyAppApi.Models
{
    
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
        public string Note { get; set; }

        
        public PaymentType PaymentType { get; set; }

     
        public int? RepairId { get; set; }
        public Repair? Repair { get; set; }

  
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
    public enum PaymentType
    {
        Pay,    
        Receive 
    }

}