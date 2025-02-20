using MyAppApi.Data.Dtos;

public class manageBookingDto
{
    public string CarModel { get; set; }

    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string IdentityImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string UserId { get; set; }
    public int CarId { get; set; }
    public string IdentityImage { get; set; }
    public string BookingStatus { get; set; }
    public UserDto User { get; internal set; }
    public CarDto Car { get; internal set; }
}
