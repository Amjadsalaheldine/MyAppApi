public class BookingDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; }
    public int CarId { get; set; }
    public string? IdentityImage { get; set; }
    public string BookingStatus { get; set; }

    public int? ModelId { get; set; }
    public string? ModelName { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }

    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

}
