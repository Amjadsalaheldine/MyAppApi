

public class RepairDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int WorkshopId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public decimal Payment { get; set; } // تمت إضافة Payment
    public string Problem { get; set; } = string.Empty;
    public string? Memo { get; set; }
    public string Responsible { get; set; } = string.Empty;
    public WorkshopDto Workshop { get; set; }
    public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>(); // قائمة الدفعات
}