namespace MyAppApi.Data.Dtos
{
    public class CarDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Location { get; internal set; }
        public string Status { get; set; } 
    }
}
