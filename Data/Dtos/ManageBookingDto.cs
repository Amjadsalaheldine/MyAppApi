public class BookingDto
{
    internal string CarModel;

    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string IdentityImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Pending"; 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalPrice { get; internal set; }
    public string UserId { get; internal set; }
    public int CarId { get; internal set; }
    public string IdentityImage { get; internal set; }
    public string BookingStatus { get; internal set; }
}
