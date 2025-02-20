public class PaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal RemainingBalance { get; set; }
    public int BookingId { get; set; }

    public decimal TotalPrice { get; set; }
}
