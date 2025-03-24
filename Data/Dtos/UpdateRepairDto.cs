
public class UpdateRepairDto
{
    public DateTime DeliveryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string Problem { get; set; } = string.Empty;
    public string? Memo { get; set; }
    public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
    public string UserName { get; set; } = string.Empty; // إضافة UserName إذا كان مطلوبًا
}