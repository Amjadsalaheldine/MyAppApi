﻿namespace MyAppApi.Models
{
    public class Booking
    {
        public int Id { get; set; }

        
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }

        
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }

      
        public decimal TotalPrice { get; set; }

        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

      
        public int CarId { get; set; }
        public Car Car { get; set; }

        
        public string IdentityImage { get; set; }

        
        public Payment Payment { get; set; }

        
        public string BookingStatus { get; set; }
        
    }
}
