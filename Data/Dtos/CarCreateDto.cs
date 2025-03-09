public class CarCreateDto
{
    public int? ModelId { get; set; }
    public int? BrandId { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int LocationId { get; set; }
    public string Status { get; set; } = "Available";
    public string PlateNumber { get; set; }
    public string EngineNumber { get; set; }
    public string ChassisNumber { get; set; }
    public string Classification { get; set; }
    public string CountryOfOrigin { get; set; }
    public string Color { get; set; }
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
}