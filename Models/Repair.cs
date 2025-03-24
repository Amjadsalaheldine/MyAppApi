public class Repair
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int WorkshopId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string Problem { get; set; } = string.Empty;
    public string? Memo { get; set; }

    // العلاقة مع Car
    public Car Car { get; set; } = null!;

    // العلاقة مع Workshop
    public Workshop Workshop { get; set; } = null!;

    // العلاقة مع Payment (مجموعة من الدفعات)
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}