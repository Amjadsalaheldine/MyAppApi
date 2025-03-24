
public class PaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal RemainingBalance { get; set; }
    public int BookingId { get; set; }
    public string PaymentMethod { get; set; }
    public string Reference { get; set; }
    public string Note { get; set; }
    public MyAppApi.Models.PaymentType PaymentType { get; set; }
    public decimal TotalPrice { get; set; }
    public string UserName { get; set; }
    public int RepairId { get; set; } // إضافة RepairId
}
public enum PaymentType
{
    Pay,   
    Receive 
}


