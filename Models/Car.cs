    namespace MyAppApi.Models
    {
        public class Car
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public string Brand { get; set; }
            public int Year { get; set; }
            public decimal Price { get; set; }

            public int LocationId { get; set; }
            public Location Location { get; set; }

            public ICollection<Booking> Bookings { get; set; }
            public ICollection<Image> Images { get; set; }

       
            public CarStatus Status { get; set; } = CarStatus.Available;
        }

   
        public enum CarStatus
        {
            Available,   
            Booked,      
            Unavailable 
        }
    }
