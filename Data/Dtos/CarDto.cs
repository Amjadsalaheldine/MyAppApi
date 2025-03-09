public class CarDto
{
    public int Id { get; set; }
    public int? ModelId { get; set; }
    public string ModelName { get; set; }
    public int? BrandId { get; set; }
    public string BrandName { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; }
    public CarStatus Status { get; set; }
    public string PlateNumber { get; set; }
    public string EngineNumber { get; set; }
    public string ChassisNumber { get; set; }
    public string Classification { get; set; }
    public string CountryOfOrigin { get; set; }
    public string Color { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}