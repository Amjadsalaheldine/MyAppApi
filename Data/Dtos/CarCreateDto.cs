namespace MyAppApi.Data.Dtos
{
    public class CarCreateDto
    {
        public string Model { get; set; }
        public string Brand { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }


        public int LocationId { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}
