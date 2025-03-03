namespace MyAppApi.Data.Dtos
{
    public class CarUpdateDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }

       
        public CarStatus Status { get; set; }
    }
}
