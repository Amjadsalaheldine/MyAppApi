namespace MyAppApi.Models
{
    public class Car
    {
        public int Id { get; set; }

        public int? ModelId { get; set; }
        public virtual Model Model { get; set; }

        public int? BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        public int Year { get; set; }
        public decimal Price { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

      
        public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();

        public CarStatus Status { get; set; } = CarStatus.Available;

        public string PlateNumber { get; set; }
        public string EngineNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string Classification { get; set; }
        public string CountryOfOrigin { get; set; }
        public string Color { get; set; }
    }

    public enum CarStatus
    {
        Available,
        Booked,
        Unavailable
    }
}